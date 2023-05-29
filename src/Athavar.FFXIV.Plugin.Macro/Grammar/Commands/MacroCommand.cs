// <copyright file="MacroCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Athavar.FFXIV.Plugin.Macro.Managers;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     The base command other commands inherit from.
/// </summary>
internal abstract class MacroCommand
{
    private static readonly Random Rand = new();
    private static IServiceProvider? serviceProvider;
    private static IDalamudServices? dalamudServices;
    private static IChatManager? chatManager;
    private static MacroManager? macroManager;
    private static MacroConfiguration? configuration;
    private static ICommandInterface? commandInterface;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroCommand"/> class.
    /// </summary>
    /// <param name="text">Original line text.</param>
    /// <param name="waitMod">Wait value.</param>
    protected MacroCommand(string text, WaitModifier waitMod)
        : this(text, waitMod.Wait, waitMod.WaitUntil)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroCommand"/> class.
    /// </summary>
    /// <param name="text">Original line text.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="until">WaitUntil value.</param>
    protected MacroCommand(string text, int wait, int until)
    {
        this.Text = text;
        this.Wait = wait;
        this.WaitUntil = until;
    }

    /// <summary>
    ///     Gets the original line text.
    /// </summary>
    public string Text { get; }

    /// <summary>
    ///     Gets the <see cref="IServiceProvider"/>.
    /// </summary>
    protected static IServiceProvider ServiceProvider => serviceProvider ?? throw new NullReferenceException("ServiceProvider is not set");

    /// <summary>
    ///     Gets the <see cref="IDalamudServices"/>.
    /// </summary>
    protected static IDalamudServices DalamudServices => dalamudServices ?? throw new NullReferenceException("DalamudServices is not set");

    /// <summary>
    ///     Gets the <see cref="IChatManager"/>.
    /// </summary>
    protected static IChatManager ChatManager => chatManager ?? throw new NullReferenceException("ChatManager is not set");

    /// <summary>
    ///     Gets the <see cref="MacroManager"/>.
    /// </summary>
    protected static MacroManager MacroManager => macroManager ?? throw new NullReferenceException("MacroManager is not set");

    /// <summary>
    ///     Gets the <see cref="MacroManager"/>.
    /// </summary>
    protected static MacroConfiguration Configuration => configuration ?? throw new NullReferenceException("MacroManager is not set");

    /// <summary>
    ///     Gets the <see cref="MacroManager"/>.
    /// </summary>
    protected static ICommandInterface CommandInterface => commandInterface ?? throw new NullReferenceException("CommandInterface is not set");

    /// <summary>
    ///     Gets the milliseconds to wait.
    /// </summary>
    protected int Wait { get; }

    /// <summary>
    ///     Gets the milliseconds to wait until.
    /// </summary>
    protected int WaitUntil { get; }

    /// <inheritdoc/>
    public override string ToString() => this.Text;

    /// <summary>
    ///     Execute a macro command.
    /// </summary>
    /// <param name="macro"></param>
    /// <param name="token">Async cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public abstract Task Execute(ActiveMacro macro, CancellationToken token);

    /// <summary>
    ///     Setup the <see cref="IServiceProvider"/> for all commands.
    /// </summary>
    /// <param name="sp">The <see cref="IServiceProvider"/>.</param>
    internal static void SetServiceProvider(IServiceProvider sp)
    {
        serviceProvider = sp;
        dalamudServices = serviceProvider.GetRequiredService<IDalamudServices>();
        chatManager = serviceProvider.GetRequiredService<IChatManager>();
        macroManager = serviceProvider.GetRequiredService<MacroManager>();
        configuration = serviceProvider.GetRequiredService<MacroConfiguration>();
        commandInterface = serviceProvider.GetRequiredService<ICommandInterface>();
    }

    /// <summary>
    ///     Perform a wait given the values in <see cref="Wait"/> and <see cref="WaitUntil"/>.
    ///     May be zero.
    /// </summary>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected async Task PerformWait(CancellationToken token)
    {
        if (this.Wait == 0 && this.WaitUntil == 0)
        {
            return;
        }

        TimeSpan sleep;
        if (this.WaitUntil == 0)
        {
            sleep = TimeSpan.FromMilliseconds(this.Wait);
            PluginLog.Debug($"Sleeping for {sleep.TotalMilliseconds} millis");
        }
        else
        {
            var value = Rand.Next(this.Wait, this.WaitUntil);
            sleep = TimeSpan.FromMilliseconds(value);
            PluginLog.Debug($"Sleeping for {sleep.TotalMilliseconds} millis ({this.Wait} to {this.WaitUntil})");
        }

        await Task.Delay(sleep, token);
    }

    /// <summary>
    ///     Perform an action every <paramref name="interval"/> seconds until either the action succeeds or
    ///     <paramref name="until"/> seconds elapse.
    /// </summary>
    /// <param name="interval">Action execution interval.</param>
    /// <param name="until">Maximum time to wait.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A value indicating whether the action succeeded.</returns>
    /// <typeparam name="T">Result type.</typeparam>
    protected async Task<(T? Result, bool Success)> LinearWait<T>(int interval, int until, Func<(T? Result, bool Success)> action, CancellationToken token)
    {
        var totalWait = 0;
        while (true)
        {
            var (result, success) = action();
            if (success)
            {
                return (result, true);
            }

            totalWait += interval;
            if (totalWait > until)
            {
                return (result, false);
            }

            await Task.Delay(interval, token);
        }
    }

    /// <summary>
    ///     Perform an action every <paramref name="interval"/> seconds until either the action succeeds or
    ///     <paramref name="until"/> seconds elapse.
    /// </summary>
    /// <param name="interval">Action execution interval.</param>
    /// <param name="until">Maximum time to wait.</param>
    /// <param name="action">Action to execute.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A value indicating whether the action succeeded.</returns>
    /// <typeparam name="T">Result type.</typeparam>
    protected async Task<bool> LinearWait(int interval, int until, Func<bool> action, CancellationToken token)
    {
        var totalWait = 0;
        while (true)
        {
            var success = action();
            if (success)
            {
                return true;
            }

            totalWait += interval;
            if (totalWait > until)
            {
                return false;
            }

            await Task.Delay(interval, token);
        }
    }
}