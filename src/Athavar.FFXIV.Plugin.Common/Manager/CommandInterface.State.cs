// <copyright file="CommandInterface.State.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;

internal sealed partial class CommandInterface
{
    private static readonly uint[] GoldenSaucerIds = { 144, 388, 389, 390, 391, 579, 792, 832, 899, 941, 1098 };

    private readonly uint logOutId;

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
    public bool IsInGoldenSaucer() => GoldenSaucerIds.Any(id => id == this.dalamudServices.ClientState.TerritoryType);

    /// <inheritdoc />
    public bool IsPvP() => this.dalamudServices.ClientState.IsPvP;

    /// <inheritdoc />
    public bool IsPlayerCharacterReady() => this.dalamudServices.Condition[ConditionFlag.NormalConditions] && !this.dalamudServices.Condition[ConditionFlag.BetweenAreas] && !this.dalamudServices.Condition[ConditionFlag.BetweenAreas51];

    /// <inheritdoc />
    public bool LogOut() => this.ExecuteMainCommand(this.logOutId);

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
        var statusId = this.dalamudServices.ClientState.LocalPlayer!.StatusList
           .Select(se => se.StatusId)
           .ToList().Intersect(statusIDs)
           .FirstOrDefault();

        return statusId != default;
    }

    private bool IsLoggedIn() => this.dalamudServices.ClientState.IsLoggedIn;
}