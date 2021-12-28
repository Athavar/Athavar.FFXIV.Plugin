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

    private readonly ManualResetEvent loggedInWaiter = new(false);
    private readonly ManualResetEvent pausedWaiter = new(true);
    private CancellationTokenSource? stepTokenSource;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    public MacroManager(IDalamudServices dalamudServices, IChatManager chatManager)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;

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
    ///     Gets a value indicating whether the manager should pause at the next /loop command.
    /// </summary>
    public bool PauseAtLoop { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the manager should stop at the next loop.
    /// </summary>
    public bool StopAtLoop { get; private set; } = false;

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
            this.pausedWaiter.Reset();
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
    public (string Name, int StepIndex)[] MacroStatus
        => this.macroStack.Select(macro => (macro.Node.Name, macro.StepIndex + 1)).ToArray();

    /// <summary>
    ///     Run a macro.
    /// </summary>
    /// <param name="node">Macro to run.</param>
    public void EnqueueMacro(MacroNode node)
    {
        this.macroStack.Push(new ActiveMacro(node));
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
            this.PauseAtLoop ^= pauseAtLoop;
            this.StopAtLoop = false;
        }
        else
        {
            this.pausedWaiter.Reset();
        }
    }

    /// <summary>
    ///     Pause at the next /loop.
    /// </summary>
    public void LoopCheckForPause()
    {
        if (this.PauseAtLoop)
        {
            this.PauseAtLoop = false;
            this.Pause(false);
        }
    }

    /// <summary>
    ///     Resume macro execution.
    /// </summary>
    public void Resume() => this.pausedWaiter.Set();
    /// <summary>
    /// Stop macro execution.
    /// </summary>
    /// <param name="stopAtLoop">Stop at the next loop instead.</param>
    public void Stop(bool stopAtLoop = false)
    {
        if (stopAtLoop)
        {
            this.PauseAtLoop = false;
            this.StopAtLoop ^= stopAtLoop;
        }
        else
        {
            this.Clear();
        }
    }

    /// <summary>
    /// Stop at the next /loop.
    /// </summary>
    public void LoopCheckForStop()
    {
        if (this.StopAtLoop)
        {
            this.StopAtLoop = false;
            this.Stop(false);
        }
    }

    /// <summary>
    ///     Loop the currently executing macro.
    /// </summary>
    public void Loop() => this.macroStack.Peek().Loop();

    /// <summary>
    ///     Clear the executing macro list.
    /// </summary>
    public void Clear()
    {
        this.macroStack.Clear();
        this.pausedWaiter.Set();
        if (!this.stepTokenSource?.IsCancellationRequested ?? false)
        {
            Task.Run(() => this.stepTokenSource?.Cancel());
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
}