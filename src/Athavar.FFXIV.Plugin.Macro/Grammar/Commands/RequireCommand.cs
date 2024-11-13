// <copyright file="RequireCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Lumina.Excel.Sheets;

/// <summary>
///     The /require command.
/// </summary>
[MacroCommand("require", null, "Require a certain effect to be present before continuing.", ["wait", "maxwait"], ["/require \"Well Fed\""], RequireLogin = true)]
internal class RequireCommand : MacroCommand
{
    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new(@"^/require\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly uint[] statusIDs;
    private readonly int maxWait;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequireCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="statusName">Status name.</param>
    /// <param name="waitMod">Wait value.</param>
    /// <param name="maxWait">MaxWait value.</param>
    private RequireCommand(string text, string statusName, WaitModifier waitMod, MaxWaitModifier maxWait)
        : base(text, waitMod)
    {
        statusName = statusName.ToLowerInvariant();
        var sheet = DalamudServices.DataManager.GetExcelSheet<Status>();
        this.statusIDs = sheet
           .Where(row => row.Name.ExtractText().ToLowerInvariant() == statusName)
           .Select(row => row.RowId)
           .ToArray();

        this.maxWait = maxWait.Wait == 0
            ? StatusCheckMaxWait
            : maxWait.Wait;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RequireCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = match.ExtractAndUnquote("name");

        return new RequireCommand(text, nameValue, waitModifier, maxWaitModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        this.Logger.Debug($"Executing: {this.Text}");
        bool IsStatusPresent() => this.CommandInterface.HasStatusId(this.statusIDs);

        var hasStatus = await this.LinearWait(StatusCheckInterval, this.maxWait, IsStatusPresent, token);

        if (!hasStatus)
        {
            throw new MacroPause("Status effect not found", IChatManager.UiColor.Red);
        }

        await this.PerformWait(token);
    }
}