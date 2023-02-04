// <copyright file="CommandInterface.State.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;

internal partial class CommandInterface
{
    private static readonly uint[] GoldenSaucerIDs = { 144, 388, 389, 390, 391, 579, 792, 899, 941 };

    /// <inheritdoc />
    public ushort GetCurrentTerritory() => this.dalamudServices.ClientState.TerritoryType;

    /// <inheritdoc />
    public byte GetCurrentJob() => (byte?)this.dalamudServices.ClientState.LocalPlayer?.ClassJob.Id ?? 0;

    /// <inheritdoc />
    public string? GetCurrentTarget() => this.dalamudServices.TargetManager.Target?.Name.ToString();

    /// <inheritdoc />
    public string? GetCurrentFocusTarget() => this.dalamudServices.TargetManager.FocusTarget?.Name.ToString();

    /// <inheritdoc />
    public bool IsInCombat() => this.dalamudServices.Condition[ConditionFlag.InCombat];

    /// <inheritdoc />
    public bool IsInDuty() => this.dalamudServices.Condition[ConditionFlag.BoundByDuty];

    /// <inheritdoc />
    public bool IsPerforming() => this.dalamudServices.Condition[ConditionFlag.Performing];

    /// <inheritdoc />
    public bool IsInGoldenSaucer() => GoldenSaucerIDs.Any(id => id == this.dalamudServices.ClientState.TerritoryType);

    /// <inheritdoc />
    public bool HasStatus(string statusName)
    {
        statusName = statusName.ToLowerInvariant();
        var sheet = this.dalamudServices.DataManager.GetExcelSheet<Status>()!;
        var statusIDs = sheet
           .Where(row => row.Name.RawString.ToLowerInvariant() == statusName)
           .Select(row => row.RowId)
           .ToArray()!;

        return this.HasStatusId(statusIDs);
    }

    /// <inheritdoc />
    public bool HasStatusId(params uint[] statusIDs)
    {
        var statusID = this.dalamudServices.ClientState.LocalPlayer!.StatusList
           .Select(se => se.StatusId)
           .ToList().Intersect(statusIDs)
           .FirstOrDefault();

        return statusID != default;
    }
}