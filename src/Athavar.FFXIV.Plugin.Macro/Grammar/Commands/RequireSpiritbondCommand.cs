// <copyright file="RequireSpiritbondCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Globalization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Dalamud.Logging;

/// <summary>
///     The /requiremateria command.
/// </summary>
[MacroCommand("requirespiritbond", null, "Pause when an item is ready to have materia extracted. Optional argument to keep crafting if the next highest spiritbond is greater-than-or-equal to the argument value.", new[] { "wait" }, new[] { "/requirespiritbond", "/requirespiritbond 99.5" })]
internal class RequireSpiritbondCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/requirespiritbond(\s+(?<within>\d+(?:\.\d+)?))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly float within;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequireSpiritbondCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="within">Check if other items are within a certain percentage.</param>
    /// <param name="wait">Wait value.</param>
    private RequireSpiritbondCommand(string text, float within, WaitModifier wait)
        : base(text, wait)
        => this.within = within;

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RequireSpiritbondCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var withinGroup = match.Groups["within"];
        var withinValue = withinGroup.Success ? withinGroup.Value : "100";
        var within = float.Parse(withinValue, CultureInfo.InvariantCulture);

        return new RequireSpiritbondCommand(text, within, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text} with {this.within}");

        if (CommandInterface.CanExtractMateria(this.within))
        {
            throw new MacroPause("You can extract materia now", IChatManager.UiColor.Green);
        }

        await this.PerformWait(token);
    }
}