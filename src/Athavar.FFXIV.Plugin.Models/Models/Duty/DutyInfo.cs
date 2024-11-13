// <copyright file="DutyInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Duty;

using Lumina.Excel.Sheets;

public sealed class DutyInfo
{
    public TerritoryIntendedUse IntendedUse => (TerritoryIntendedUse)this.TerritoryType.TerritoryIntendedUse.RowId;

    public required TerritoryType TerritoryType { get; init; }

    public required ContentRoulette ContentRoulette { get; init; }

    public required ContentCondition ActiveContentCondition { get; init; }

    public bool JoinInProgress { get; init; }

    public int QueuePlayerCount { get; init; }
}