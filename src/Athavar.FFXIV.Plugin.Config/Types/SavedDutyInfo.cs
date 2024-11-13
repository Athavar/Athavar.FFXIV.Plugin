// <copyright file="SavedDutyInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Duty;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

public sealed class SavedDutyInfo
{
    public SavedDutyInfo()
    {
    }

    public SavedDutyInfo(DutyInfo info, DateTimeOffset dateTimeOffset)
    {
        this.TerritoryTypeId = info.TerritoryType.RowId;
        this.ContentRouletteId = info.ContentRoulette.RowId;
        this.ActiveContentCondition = info.ActiveContentCondition;
        this.JoinInProgress = info.JoinInProgress;
        this.QueuePlayerCount = info.QueuePlayerCount;
        this.DutyStartTime = dateTimeOffset;
    }

    [JsonPropertyName("TerritoryTypeId")]
    public uint TerritoryTypeId { get; set; }

    [JsonPropertyName("ContentRouletteId")]
    public uint ContentRouletteId { get; set; }

    [JsonPropertyName("ActiveContentCondition")]
    public ContentCondition ActiveContentCondition { get; set; }

    [JsonPropertyName("JoinInProgress")]
    public bool JoinInProgress { get; set; }

    [JsonPropertyName("QueuePlayerCount")]
    public int QueuePlayerCount { get; set; }

    [JsonPropertyName("DutyStartTime")]
    public DateTimeOffset DutyStartTime { get; set; }

    public DutyInfo? GetDutyInfo(IDataManager dataManager)
    {
        if (dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(this.TerritoryTypeId) is not { } territoryType)
        {
            return null;
        }

        if (dataManager.Excel.GetSheet<ContentRoulette>()?.GetRow(this.ContentRouletteId) is not { } contentRoulette)
        {
            return null;
        }

        return new DutyInfo
        {
            TerritoryType = territoryType,
            ContentRoulette = contentRoulette,
            ActiveContentCondition = this.ActiveContentCondition,
            JoinInProgress = this.JoinInProgress,
            QueuePlayerCount = this.QueuePlayerCount,
        };
    }
}