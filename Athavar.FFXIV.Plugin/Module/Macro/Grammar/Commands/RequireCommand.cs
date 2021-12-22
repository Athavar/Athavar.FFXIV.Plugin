// <copyright file="RequireCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using Lumina.Excel.GeneratedSheets;

/// <summary>
///     The /require command.
/// </summary>
internal class RequireCommand : MacroCommand
{
    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new(@"^/require\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly List<uint> statusIDs;
    private readonly int maxWait;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RequireCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="statusName">Status name.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="maxWait">MaxWait value.</param>
    private RequireCommand(string text, string statusName, WaitModifier wait, MaxWaitModifier maxWait)
        : base(text, wait)
    {
        statusName = statusName.ToLowerInvariant();
        var sheet = dalamudServices.DataManager.GetExcelSheet<Status>()!;
        this.statusIDs = sheet
                        .Where(row => row.Name.RawString.ToLowerInvariant() == statusName)
                        .Select(row => row.RowId)
                        .ToList()!;

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

        var nameValue = ExtractAndUnquote(match, "name");

        return new RequireCommand(text, nameValue, waitModifier, maxWaitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        var (statusId, hasStatus) = await this.LinearWait(StatusCheckInterval, this.maxWait, this.IsStatusPresent, token);

        if (!hasStatus)
        {
            throw new MacroCommandError("Status effect not found");
        }

        await this.PerformWait(token);
    }

    private (uint StatusID, bool HasStatus) IsStatusPresent()
    {
        var statusId = dalamudServices.ClientState.LocalPlayer!.StatusList
                                      .Select(se => se.StatusId)
                                      .ToList().Intersect(this.statusIDs)
                                      .FirstOrDefault();

        if (statusId == default)
        {
            return (0, false);
        }

        return (statusId, true);
    }
}