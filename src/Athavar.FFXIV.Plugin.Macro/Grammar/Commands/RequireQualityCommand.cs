// <copyright file="RequireQualityCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Globalization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;

/// <summary>
///     The /requirequality command.
/// </summary>
[MacroCommand("requirequality", null, "Require a certain amount of quality be present before continuing.", new[] { "wait", "maxwait" }, new[] { "/requirequality 3000" }, RequireLogin = true)]
internal class RequireQualityCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/requirequality\s+(?<quality>\d+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint requiredQuality;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequireQualityCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="quality">Quality value.</param>
    /// <param name="wait">Wait value.</param>
    private RequireQualityCommand(string text, uint quality, WaitModifier wait)
        : base(text, wait)
        => this.requiredQuality = quality;

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RequireQualityCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var qualityValue = match.Groups["quality"].Value;
        var quality = uint.Parse(qualityValue, CultureInfo.InvariantCulture);

        return new RequireQualityCommand(text, quality, waitModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        this.Logger.Debug($"Executing: {this.Text}");

        var current = this.CommandInterface.GetQuality();
        if (current < this.requiredQuality)
        {
            throw new MacroPause("Required quality was not found", IChatManager.UiColor.Red);
        }

        await this.PerformWait(token);
    }
}