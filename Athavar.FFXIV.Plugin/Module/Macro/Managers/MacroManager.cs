// <copyright file="MacroManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    private readonly MacroConfiguration configuration;
    private readonly CancellationTokenSource eventLoopTokenSource = new();
    private readonly Stack<ActiveMacro> macroStack = new();

    private readonly ManualResetEvent loggedInWaiter = new(false);
    private readonly ManualResetEvent pausedWaiter = new(true);
    private CancellationTokenSource? stepTokenSource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    public MacroManager(IDalamudServices dalamudServices, IChatManager chatManager, Configuration configuration)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.configuration = configuration.Macro!;

        this.dalamudServices.ClientState.Login += this.OnLogin;
        this.dalamudServices.ClientState.Logout += this.OnLogout;

        if (this.dalamudServices.ClientState.LocalPlayer != null)
        {
            this.loggedInWaiter.Set();
        }

        // Start the loop.
        Task.Factory.StartNew(this.EventLoop, TaskCreationOptions.LongRunning);
    }

    private delegate IntPtr EventFrameworkDelegate(IntPtr a1, IntPtr a2, uint a3, ushort a4, IntPtr a5, IntPtr dataPtr, byte dataSize);

    /// <summary>
    ///     Gets the state of the macro manager.
    /// </summary>
    public LoopState State { get; private set; } = LoopState.Waiting;

    /// <summary>
    ///     Gets a value indicating whether the manager should pause at the next loop.
    /// </summary>
    public bool PauseAtLoop { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the manager should stop at the next loop.
    /// </summary>
    public bool StopAtLoop { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamudServices.ClientState.Login -= this.OnLogin;
        this.dalamudServices.ClientState.Logout -= this.OnLogout;

        this.eventLoopTokenSource.Cancel();
        this.eventLoopTokenSource.Dispose();

        this.loggedInWaiter.Dispose();
        this.pausedWaiter.Dispose();
    }

    private void OnLogin(object? sender, EventArgs e)
    {
        this.loggedInWaiter.Set();
        this.State = LoopState.Waiting;
    }

    private void OnLogout(object? sender, EventArgs e)
    {
        this.loggedInWaiter.Reset();
        this.State = LoopState.NotLoggedIn;
    }

    private async Task EventLoop()
    {
        var token = this.eventLoopTokenSource.Token;

        while (!token.IsCancellationRequested)
        {
            try
            {
                // Check if the logged in waiter is set
                if (!this.loggedInWaiter.WaitOne(0))
                {
                    this.State = LoopState.NotLoggedIn;

                    this.macroStack.Clear();
                }

                // Wait to be logged in
                this.loggedInWaiter.WaitOne();

                // Check if the paused waiter has been set
                if (!this.pausedWaiter.WaitOne(0))
                {
                    this.State = this.macroStack.Count == 0
                        ? LoopState.Waiting
                        : LoopState.Paused;
                }

                // Wait for the un-pause button
                this.pausedWaiter.WaitOne();

                if (!this.macroStack.TryPeek(out var macro))
                {
                    this.pausedWaiter.Reset();
                    continue;
                }

                this.State = LoopState.Running;

                using var macroToken = CancellationTokenSource.CreateLinkedTokenSource(token);

                this.stepTokenSource = macroToken;
                if (await this.ProcessMacro(macro, this.stepTokenSource.Token))
                {
                    this.macroStack.Pop();
                }

                this.stepTokenSource = null;
            }
            catch (OperationCanceledException)
            {
                PluginLog.Verbose("Event loop has been cancelled");
                this.State = LoopState.Stopped;
                break;
            }
            catch (ObjectDisposedException)
            {
                PluginLog.Verbose("Event loop has been disposed");
                this.State = LoopState.Stopped;
                break;
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Unhandled exception occurred");
                this.chatManager.PrintErrorMessage("[Athavar.Macro] Worker has encountered an accident.");
                this.macroStack.Clear();
                this.PlayErrorSound();
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
        catch (GateComplete)
        {
            return true;
        }
        catch (MacroCommandError ex)
        {
            this.chatManager.PrintErrorMessage($"{ex.Message}: Failure while running {step} (step {macro.StepIndex + 1})");
            this.pausedWaiter.Reset();
            this.PlayErrorSound();
            return false;
        }
        catch (OperationCanceledException)
        {
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
    public (string Name, int StepIndex)[] MacroStatus => this.macroStack.Select(macro => (macro.Node.Name, macro.StepIndex + 1)).ToArray();

    /// <summary>
    ///     Run a macro.
    /// </summary>
    /// <param name="node">Macro to run.</param>
    public void EnqueueMacro(MacroNode node)
    {
        var contents = this.ModifyMacroForCraftLoop(node.Contents, node.CraftingLoop, node.CraftLoopCount);
        this.macroStack.Push(new ActiveMacro(node, contents));
        this.pausedWaiter.Set();
    }

    /// <summary>
    ///     Pause macro execution.
    /// </summary>
    /// <param name="pauseAtLoop">Pause at the next loop instead.</param>
    public void Pause(bool pauseAtLoop = false)
    {
        if (pauseAtLoop)
        {
            this.PauseAtLoop ^= true;
            this.StopAtLoop = false;
        }
        else
        {
            this.PauseAtLoop = false;
            this.StopAtLoop = false;
            this.pausedWaiter.Reset();
            this.chatManager.Clear();
        }
    }

    /// <summary>
    ///     Pause at the next /loop.
    /// </summary>
    public void LoopCheckForPause()
    {
        if (this.PauseAtLoop)
        {
            this.Pause();
        }
    }

    /// <summary>
    ///     Resume macro execution.
    /// </summary>
    public void Resume() => this.pausedWaiter.Set();

    /// <summary>
    ///     Stop macro execution.
    /// </summary>
    /// <param name="stopAtLoop">Stop at the next loop instead.</param>
    public void Stop(bool stopAtLoop = false)
    {
        if (stopAtLoop)
        {
            this.PauseAtLoop = false;
            this.StopAtLoop ^= true;
        }
        else
        {
            this.PauseAtLoop = false;
            this.StopAtLoop = false;
            this.pausedWaiter.Set();
            this.macroStack.Clear();
            this.chatManager.Clear();
        }
    }

    /// <summary>
    ///     Stop at the next /loop.
    /// </summary>
    public void LoopCheckForStop()
    {
        if (this.StopAtLoop)
        {
            this.StopAtLoop = false;
            this.Stop();
        }
    }

    /// <summary>
    ///     Loop the currently executing macro.
    /// </summary>
    public void Loop()
    {
        if (this.macroStack.TryPeek(out var macro))
        {
            // While there should always be a macro present, the
            // stack can be empty if it is cleared during a loop.
            macro.Loop();
        }
    }

    /// <summary>
    ///     Proceed to the next step.
    /// </summary>
    public void NextStep()
    {
        if (this.macroStack.TryPeek(out var macro))
        {
            // While there should always be a macro present, the
            // stack can be empty if it is cleared during a loop.
            macro.NextStep();
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

        return this.macroStack.Peek().Steps.Select(s => s.ToString()).ToArray();
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

    /// <summary>
    ///     Modify a macro for craft looping.
    /// </summary>
    /// <param name="contents">Contents of a macroNode.</param>
    /// <param name="craftLoop">A value indicating whether craftLooping is enabled.</param>
    /// <param name="craftCount">Amount to craftLoop.</param>
    /// <returns>The modified macro.</returns>
    public string ModifyMacroForCraftLoop(string contents, bool craftLoop, int craftCount)
    {
        if (!craftLoop)
        {
            return contents;
        }

        var sb = new StringBuilder();

        var maxwait = this.configuration.CraftLoopMaxWait;
        var maxwaitMod = maxwait > 0 ? $" <maxwait.{maxwait}>" : string.Empty;

        var echo = this.configuration.CraftLoopEcho;
        var echoMod = echo ? " <echo>" : string.Empty;

        var craftGateStep = this.configuration.CraftLoopFromRecipeNote
            ? $"/craft {craftCount}{echoMod}"
            : $"/gate {craftCount - 1}{echoMod}";

        var clickSteps = string.Join("\n", $@"/waitaddon ""RecipeNote""{maxwaitMod}", @"/click ""synthesize""", $@"/waitaddon ""Synthesis""{maxwaitMod}");

        var loopStep = $"/loop{echoMod}";

        if (this.configuration.CraftLoopFromRecipeNote)
        {
            if (craftCount == -1)
            {
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
                sb.AppendLine(loopStep);
            }
            else if (craftCount == 0)
            {
                sb.AppendLine(contents);
            }
            else if (craftCount == 1)
            {
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
            }
            else
            {
                sb.AppendLine(craftGateStep);
                sb.AppendLine(clickSteps);
                sb.AppendLine(contents);
                sb.AppendLine(loopStep);
            }
        }
        else
        {
            if (craftCount == -1)
            {
                sb.AppendLine(contents);
                sb.AppendLine(clickSteps);
                sb.AppendLine(loopStep);
            }
            else if (craftCount == 0 || craftCount == 1)
            {
                sb.AppendLine(contents);
            }
            else
            {
                sb.AppendLine(contents);
                sb.AppendLine(craftGateStep);
                sb.AppendLine(clickSteps);
                sb.AppendLine(loopStep);
            }
        }

        return sb.ToString().Trim();
    }

    private void PlayErrorSound()
    {
        if (!this.configuration.NoisyErrors)
        {
            return;
        }

        for (var i = 0; i < 3; i++)
        {
            Console.Beep(900, 250);
        }
    }
}