// <copyright file="DutyInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Models;

using Athavar.FFXIV.Plugin.Models;
using Lumina.Excel.GeneratedSheets;

public sealed class DutyInfo
{
    public TerritoryIntendedUse IntendedUse => (TerritoryIntendedUse)this.TerritoryType.TerritoryIntendedUse;

    public required TerritoryType TerritoryType { get; init; }

    public required ContentRoulette ContentRoulette { get; init; }

    public required ContentCondition ActiveContentCondition { get; init; }

    public bool JoinInProgress { get; init; }

    public int QueuePlayerCount { get; init; }
}