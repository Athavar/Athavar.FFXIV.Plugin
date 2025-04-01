// <copyright file="ContentEncounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Data;

using Lumina.Excel.Sheets;

public sealed class ContentEncounter
{
    public TimeSpan Duration => this.EndDate == DateTimeOffset.MinValue ? DateTimeOffset.Now - this.StartDate : this.EndDate - this.StartDate;

    public long Id { get; init; }

    public required uint ContentRouletteId { get; init; }

    public ContentRoulette? ContentRoulette { get; set; }

    public required uint TerritoryTypeId { get; init; }

    public TerritoryType? TerritoryType { get; set; }

    public MultiString? ContentFinderCondition { get; set; }

    public ulong PlayerContentId { get; set; }

    public required uint ClassJobId { get; init; }

    public required bool Completed { get; set; }

    public required DateTimeOffset StartDate { get; init; } = DateTimeOffset.MinValue;

    public required DateTimeOffset EndDate { get; init; } = DateTimeOffset.MinValue;

    public ContentCondition ActiveContentCondition { get; set; }

    public required bool JoinInProgress { get; init; } = false;

    public required int QueuePlayerCount { get; init; }

    public required int Wipes { get; init; }

    public required int PlayerDeathCount { get; init; }

    public required bool TrackingWasInterrupted { get; init; } = false;
}