// <copyright file="MacroManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Dalamud.Logging;

/// <summary>
///     Manager that handles running macros.
/// </summary>
internal partial class MacroManager : IDisposable
{
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly CancellationTokenSource eventLoopTokenSource = new();
    private readonly Stack<ActiveMacro> macroStack = new();

    private CancellationTokenSource? stepTokenSource;
    private bool isPaused = true;
    private bool isLoggedIn;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    public MacroManager(IServiceProvider provider, IDalamudServices dalamudServices, IChatManager chatManager)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;

        this.dalamudServices.ClientState.Login += this.OnLogin;
        this.dalamudServices.ClientState.Logout += this.OnLogout;

        if (this.dalamudServices.ClientState.LocalPlayer != null)
        {
            this.isLoggedIn = true;
        }

        // Start the loop.
        Task.Factory.StartNew(this.EventLoop, TaskCreationOptions.LongRunning);
    }

    private delegate IntPtr EventFrameworkDelegate(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize);

    /// <summary>
    ///     Gets the state of the macro manager.
    /// </summary>
    public LoopState State { get; private set; } = LoopState.Waiting;

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamudServices.ClientState.Login -= this.OnLogin;
        this.dalamudServices.ClientState.Logout -= this.OnLogout;

        this.eventLoopTokenSource.Cancel();
        this.eventLoopTokenSource.Dispose();
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

    private async Task EventLoop()
    {
        var token = this.eventLoopTokenSource.Token;

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
                    this.State = this.macroStack.Count == 0 ? LoopState.Waiting : LoopState.Paused;
                    await Task.Delay(100, token);
                }

                if (!this.macroStack.TryPeek(out var macro))
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
                    this.macroStack.Pop();
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
                this.chatManager.PrintErrorMessage("[Athavar.Macro] Worker has died unexpectedly.");
                this.macroStack.Clear();
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

        try
        {
            await step.Execute(token);
        }
        catch (MacroCommandError ex)
        {
            this.chatManager.PrintErrorMessage($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
            this.isPaused = true;
            return false;
        }

        macro.NextStep();

        return false;
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
    public int MacroCount => this.macroStack.Count;

    /// <summary>
    ///     Gets the name and currently executing line of each active macro.
    /// </summary>
    public (string Name, int StepIndex)[] MacroStatus
        => this.macroStack.Select(macro => (macro.Node.Name, macro.StepIndex + 1)).ToArray();

    /// <summary>
    ///     Run a macro.
    /// </summary>
    /// <param name="node">Macro to run.</param>
    public void EnqueueMacro(MacroNode node)
    {
        this.macroStack.Push(new ActiveMacro(node));

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
    ///     Loop the currently executing macro.
    /// </summary>
    public void Loop() => this.macroStack.Peek().Loop();

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
        this.macroStack.Clear();
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
        if (this.macroStack.Count == 0)
        {
            return Array.Empty<string>();
        }

        return this.macroStack.Peek().Steps.Select(s => s.Text).ToArray();
    }

    /// <summary>
    ///     Gets the executing line number of the current macro.
    /// </summary>
    /// <returns>Macro line number.</returns>
    public int CurrentMacroStep()
    {
        if (this.macroStack.Count == 0)
        {
            return 0;
        }

        return this.macroStack.First().StepIndex;
    }
}