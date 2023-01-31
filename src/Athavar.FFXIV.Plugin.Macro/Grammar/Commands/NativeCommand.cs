// <copyright file="NativeCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Dalamud.Logging;

/// <summary>
///     A command handled by the game.
/// </summary>
internal class NativeCommand : MacroCommand
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NativeCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="waitMod">Wait value.</param>
    private NativeCommand(string text, WaitModifier waitMod)
        : base(text, waitMod)
    {
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static NativeCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        return new NativeCommand(text, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        ChatManager.SendMessage(this.Text);

        await this.PerformWait(token);
    }
}