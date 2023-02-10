// <copyright file="RequireStatsCommand.cs" company="Athavar">
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
///     The /requirestats command.
/// </summary>
[MacroCommand("requirestats", null, "Require a certain amount of stats effect to be present before continuing. Syntax is Craftsmanship, Control, then CP.", new[] { "wait", "maxwait" }, new[] { "/requirestats 2700 2600 500" })]
internal class RequireStatsCommand : MacroCommand
{
    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new(@"^/requirestats\s+(?<craftsmanship>\d+)\s+(?<control>\d+)\s+(?<cp>\d+)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint requiredCraftsmanship;
    private readonly uint requiredControl;
    private readonly uint requiredCp;
    private readonly int maxWait;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequireStatsCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="craftsmanship">Craftsmanship value.</param>
    /// <param name="control">Control value.</param>
    /// <param name="cp">Cp value.</param>
    /// <param name="waitMod">Wait value.</param>
    /// <param name="maxWait">MaxWait value.</param>
    private RequireStatsCommand(string text, uint craftsmanship, uint control, uint cp, WaitModifier waitMod, MaxWaitModifier maxWait)
        : base(text, waitMod)
    {
        this.requiredCraftsmanship = craftsmanship;
        this.requiredControl = control;
        this.requiredCp = cp;

        this.maxWait = maxWait.Wait == 0
            ? StatusCheckMaxWait
            : maxWait.Wait;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RequireStatsCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var craftsmanshipValue = match.Groups["craftsmanship"].Value;
        var craftsmanship = uint.Parse(craftsmanshipValue, CultureInfo.InvariantCulture);

        var controlValue = match.Groups["control"].Value;
        var control = uint.Parse(controlValue, CultureInfo.InvariantCulture);

        var cpValue = match.Groups["cp"].Value;
        var cp = uint.Parse(cpValue, CultureInfo.InvariantCulture);

        return new RequireStatsCommand(text, craftsmanship, control, cp, waitModifier, maxWaitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        bool AreStatsGood() => CommandInterface.HasStats(this.requiredCraftsmanship, this.requiredControl, this.requiredCp);
        var hasStats = await this.LinearWait(StatusCheckInterval, this.maxWait, AreStatsGood, token);

        if (!hasStats)
        {
            throw new MacroPause("Required stats were not found", IChatManager.UiColor.Yellow);
        }

        await this.PerformWait(token).ConfigureAwait(false);
    }
}