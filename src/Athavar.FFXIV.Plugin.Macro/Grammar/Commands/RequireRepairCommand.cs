// <copyright file="RequireRepairCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Dalamud.Logging;

/// <summary>
///     The /requirerepair command.
/// </summary>
[MacroCommand("requirerepair", null, "Pause if an item is at zero durability.", new[] { "wait" }, new[] { "/requirerepair" })]
internal class RequireRepairCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/requirerepair\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequireRepairCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="wait">Wait value.</param>
    private RequireRepairCommand(string text, WaitModifier wait)
        : base(text, wait)
    {
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RequireRepairCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        return new RequireRepairCommand(text, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        if (CommandInterface.NeedsRepair())
        {
            throw new MacroPause("You need to repair", IChatManager.UiColor.Yellow);
        }

        await this.PerformWait(token);
    }
}