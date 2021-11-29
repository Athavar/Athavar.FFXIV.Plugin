// <copyright file="MacroManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Commands;
using Athavar.FFXIV.Plugin.Module.Macro.CraftingData;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Dalamud.Game.Command;
using Dalamud.Hooking;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Manager that handles running macros.
/// </summary>
internal partial class MacroManager : IDisposable
{
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly CancellationTokenSource eventLoopTokenSource = new();
    private readonly List<ActiveMacro> runningMacros = new();
    private readonly Hook<EventFrameworkDelegate> eventFrameworkHook;

    private readonly Dictionary<string, BaseCommand> commands = new();
    private CancellationTokenSource? stepTokenSource;
    private bool isPaused = true;
    private bool isLoggedIn;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    /// <param name="addressResolver"><see cref="PluginAddressResolver" /> added by DI.</param>
    /// <param name="serviceProvider"><see cref="IServiceProvider" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    public MacroManager(IDalamudServices dalamudServices, IChatManager chatManager, PluginAddressResolver addressResolver, IServiceProvider serviceProvider, Configuration configuration)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.Configuration = configuration.Macro!;

        this.dalamudServices.ClientState.Login += this.OnLogin;
        this.dalamudServices.ClientState.Logout += this.OnLogout;

        this.PopulateCommands(serviceProvider);

        if (this.dalamudServices.ClientState.LocalPlayer != null)
        {
            this.isLoggedIn = true;
        }

        this.eventFrameworkHook = new Hook<EventFrameworkDelegate>(addressResolver.EventFrameworkFunctionAddress, this.EventFrameworkDetour);
        this.eventFrameworkHook.Enable();

        Task.Run(() => this.EventLoop(this.eventLoopTokenSource.Token));

        this.dalamudServices.CommandManager.AddHandler("/runmacro", new CommandInfo(this.CommandHandler)
                                                                    {
                                                                        HelpMessage = "Start run a macro of the macro module.",
                                                                        ShowInHelp = true,
                                                                    });
    }

    private delegate IntPtr EventFrameworkDelegate(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize);

    /// <summary>
    ///     Gets the <see cref="MacroConfiguration" />.
    /// </summary>
    public MacroConfiguration Configuration { get; }

    /// <summary>
    ///     Gets the state of the macro manager.
    /// </summary>
    public LoopState State { get; private set; } = LoopState.Waiting;

    /// <summary>
    ///     Gets the current crafting data.
    /// </summary>
    public CraftingState CraftingData { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamudServices.CommandManager.RemoveHandler("/runmacro");

        this.dalamudServices.ClientState.Login -= this.OnLogin;
        this.dalamudServices.ClientState.Logout -= this.OnLogout;

        this.eventLoopTokenSource.Cancel();
        this.eventLoopTokenSource.Dispose();
        this.eventFrameworkHook.Dispose();

        // Dispose disposeable commands.
        foreach (var command in this.commands.Select(c => c.Value as IDisposable).Where(d => d is not null))
        {
            command?.Dispose();
        }
    }

    private void CommandHandler(string command, string arguments)
    {
        var args = arguments.Trim().Replace("\"", string.Empty);

        switch (this.State)
        {
            case LoopState.Waiting:
            {
                var node = this.Configuration.GetAllNodes().FirstOrDefault(node => node.Name == args);

                if (node is MacroNode macro)
                {
                    this.RunMacro(macro);
                }
                else
                {
                    this.chatManager.PrintError($"Fail to find macro with name: {args}");
                }

                break;
            }

            default:
                this.chatManager.PrintError($"Fail to run macro: {this.State}");
                break;
        }
    }

    private IntPtr EventFrameworkDetour(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize)
    {
        try
        {
            if (dataSize >= 4)
            {
                var dataType = (ActionCategory)Marshal.ReadInt32(dataPtr);
                if (dataType == ActionCategory.Action || dataType == ActionCategory.CraftAction)
                {
                    this.CraftingData = Marshal.PtrToStructure<CraftingState>(dataPtr);
                }
            }
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Don't crash the game.");
        }

        return this.eventFrameworkHook.Original(a1, a2, a3, a4, a5, dataPtr, dataSize);
    }

    private void OnLogin(object? sender, EventArgs e)
    {
        this.isLoggedIn = true;
        this.State = LoopState.Waiting;
    }

    private void OnLogout(object? sender, EventArgs e)
    {
        this.isLoggedIn = false;
        this.State = LoopState.NotLoggedIn;
    }

    private async Task EventLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                while (!this.isLoggedIn)
                {
                    this.State = LoopState.NotLoggedIn;
                    await Task.Delay(100, token);
                }

                while (this.isPaused)
                {
                    this.State = this.runningMacros.Count == 0 ? LoopState.Waiting : LoopState.Paused;
                    await Task.Delay(100, token);
                }

                var macro = this.runningMacros.FirstOrDefault();
                if (macro == default(ActiveMacro))
                {
                    this.isPaused = true;
                    continue;
                }

                if (this.State != LoopState.Cancel)
                {
                    this.State = LoopState.Running;
                }

                using var macroToken = CancellationTokenSource.CreateLinkedTokenSource(this.eventLoopTokenSource.Token);

                this.stepTokenSource = macroToken;
                if (await this.ProcessMacro(macro, this.stepTokenSource.Token))
                {
                    this.runningMacros.Remove(macro);
                }

                this.stepTokenSource = null;
            }
            catch (OperationCanceledException)
            {
                PluginLog.Verbose("Event loop has stopped");
                this.State = LoopState.Stopped;
                break;
            }
            catch (ObjectDisposedException)
            {
                PluginLog.Verbose("Event loop has stopped");
                this.State = LoopState.Stopped;
                break;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Unhandled exception occurred");
                this.chatManager.PrintError("[Athavar.Macro] Worker has died unexpectedly.");
                this.runningMacros.Clear();
            }
        }
    }

    private async Task<bool> ProcessMacro(ActiveMacro macro, CancellationToken token)
    {
        var step = macro.GetCurrentStep();

        if (step == null)
        {
            return true;
        }

        var wait = BaseCommand.ExtractWait(ref step);

        try
        {
            var command = step.ToLower().Split(' ').First();

            if (command.StartsWith('/') && this.commands.TryGetValue(command.Substring(1), out var commandImp))
            {
                wait = await commandImp.Execute(step, macro, wait, token).ConfigureAwait(false);
            }
            else
            {
                this.chatManager.SendMessage(step);
            }

            if (wait.TotalSeconds > 0)
            {
                await Task.Delay(wait, token).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is EffectNotPresentError || ex is ConditionNotFulfilledError)
        {
            this.chatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
            this.isPaused = true;
            return false;
        }
        catch (EventFrameworkTimeoutError ex)
        {
            this.chatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
            this.isPaused = true;
            return false;
        }
        catch (InvalidMacroOperationException ex)
        {
            this.chatManager.PrintError($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
            this.isPaused = true;
            return true;
        }
        catch (OperationCanceledException)
        {
            this.isPaused = true;
            return true;
        }

        macro.StepIndex++;

        return false;
    }

    private void PopulateCommands(IServiceProvider serviceProvider)
    {
        var baseType = typeof(BaseCommand);
        var commandTypes = baseType.Assembly.GetTypes().Where(t => t.IsSubclassOf(baseType));

        foreach (var commandType in commandTypes)
        {
            var instance = ActivatorUtilities.CreateInstance(serviceProvider, commandType, this);

            if (instance is BaseCommand commandInstance)
            {
                foreach (var commandName in commandInstance.CommandAliases)
                {
                    this.commands[commandName] = commandInstance;
                }
            }
        }
    }
}

/// <summary>
///     Public API.
/// </summary>
internal sealed partial class MacroManager
{
    /// <summary>
    ///     Gets the amount of macros currently executing.
    /// </summary>
    public int MacroCount => this.runningMacros.Count;

    /// <summary>
    ///     Gets the name and currently executing line of each active macro.
    /// </summary>
    public (string Name, int StepIndex)[] MacroStatus
        => this.runningMacros.Select(macro => (macro.Node.Name, macro.StepIndex + 1)).ToArray();

    /// <summary>
    ///     Run a macro.
    /// </summary>
    /// <param name="node">Macro to run.</param>
    /// <param name="index">index to insert.</param>
    public void RunMacro(MacroNode node, int? index = null)
    {
        if (index.HasValue)
        {
            this.runningMacros.Insert(index.Value, new ActiveMacro(node));
        }
        else
        {
            this.runningMacros.Add(new ActiveMacro(node));
        }

        this.isPaused = false;
    }

    /// <summary>
    ///     Pause macro execution.
    /// </summary>
    public void Pause() => this.isPaused = true;

    /// <summary>
    ///     Resume macro execution.
    /// </summary>
    public void Resume() => this.isPaused = false;

    /// <summary>
    ///     Cancel macro execution.
    /// </summary>
    public void Cancel()
    {
        if (this.State == LoopState.Running)
        {
            this.State = LoopState.Cancel;
        }
    }

    /// <summary>
    ///     Clear the executing macro list.
    /// </summary>
    public void Clear()
    {
        this.runningMacros.Clear();
        if (!this.stepTokenSource?.IsCancellationRequested ?? false)
        {
            this.stepTokenSource?.Cancel();
        }
    }

    /// <summary>
    ///     Gets the contents of the current macro.
    /// </summary>
    /// <returns>Macro contents.</returns>
    public string[] CurrentMacroContent()
    {
        if (this.runningMacros.Count == 0)
        {
            return Array.Empty<string>();
        }

        return this.runningMacros.First().Steps.ToArray();
    }

    /// <summary>
    ///     Gets the executing line number of the current macro.
    /// </summary>
    /// <returns>Macro line number.</returns>
    public int CurrentMacroStep()
    {
        if (this.runningMacros.Count == 0)
        {
            return 0;
        }

        return this.runningMacros.First().StepIndex;
    }
}