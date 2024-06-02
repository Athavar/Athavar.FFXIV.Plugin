// <copyright file="ContentEncounterDto.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Dto;

using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Data;
using Dapper.Contrib.Extensions;

[Table(TableName)]
internal sealed class ContentEncounterDto : BaseDto
{
    public const string TableName = "ContentEncounter";

    public const string ColumnContentRouletteId = nameof(ContentRouletteId);
    public const string ColumnTerritoryTypeId = nameof(TerritoryTypeId);
    public const string ColumnPlayerContentId = nameof(PlayerContentId);
    public const string ColumnClassJobId = nameof(ClassJobId);
    public const string ColumnCompleted = nameof(Completed);
    public const string ColumnStartDate = nameof(StartDate);
    public const string ColumnEndDate = nameof(EndDate);
    public const string ColumnJoinInProgressEnabled = nameof(JoinInProgressEnabled);
    public const string ColumnUnrestrictedParty = nameof(UnrestrictedParty);
    public const string ColumnMinimalIL = nameof(MinimalIL);
    public const string ColumnLevelSync = nameof(LevelSync);
    public const string ColumnSilenceEcho = nameof(SilenceEcho);
    public const string ColumnExplorerMode = nameof(ExplorerMode);
    public const string ColumnJoinInProgress = nameof(JoinInProgress);
    public const string ColumnQueuePlayerCount = nameof(QueuePlayerCount);
    public const string ColumnWipes = nameof(Wipes);
    public const string ColumnPlayerDeathCount = nameof(PlayerDeathCount);
    public const string ColumnInterrupted = nameof(Interrupted);

    public const string IndexPlayerContentId = $"idx_{TableName}_{ColumnPlayerContentId}";

    public uint ContentRouletteId { get; set; }

    public uint TerritoryTypeId { get; set; }

    public ulong PlayerContentId { get; set; }

    public uint ClassJobId { get; set; }

    public bool Completed { get; set; }

    public long StartDate { get; set; }

    public long EndDate { get; set; }

    public bool JoinInProgressEnabled { get; set; }

    public bool UnrestrictedParty { get; set; }

    public bool MinimalIL { get; set; }

    public bool LevelSync { get; set; }

    public bool SilenceEcho { get; set; }

    public bool ExplorerMode { get; set; }

    public bool JoinInProgress { get; set; }

    /// <summary>
    ///     Gets or sets the amount of players queued into the content encounter.
    /// </summary>
    public int QueuePlayerCount { get; set; }

    public int Wipes { get; set; }

    public int PlayerDeathCount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the content encounter data may not be complete/correct.
    /// </summary>
    public bool Interrupted { get; set; }

    public static implicit operator ContentEncounterDto(ContentEncounter c)
        => new()
        {
            Id = c.Id,
            ContentRouletteId = c.ContentRouletteId,
            TerritoryTypeId = c.TerritoryTypeId,
            PlayerContentId = c.PlayerContentId,
            ClassJobId = c.ClassJobId,
            Completed = c.Completed,
            StartDate = c.StartDate.ToUnixTimeMilliseconds(),
            EndDate = c.EndDate.ToUnixTimeMilliseconds(),
            JoinInProgressEnabled = (c.ActiveContentCondition & ContentCondition.JoinInProgress) != 0,
            UnrestrictedParty = (c.ActiveContentCondition & ContentCondition.UnrestrictedParty) != 0,
            MinimalIL = (c.ActiveContentCondition & ContentCondition.MinimalIL) != 0,
            LevelSync = (c.ActiveContentCondition & ContentCondition.LevelSync) != 0,
            SilenceEcho = (c.ActiveContentCondition & ContentCondition.SilenceEcho) != 0,
            ExplorerMode = (c.ActiveContentCondition & ContentCondition.ExplorerMode) != 0,
            JoinInProgress = c.JoinInProgress,
            QueuePlayerCount = c.QueuePlayerCount,
            Wipes = c.Wipes,
            PlayerDeathCount = c.PlayerDeathCount,
            Interrupted = c.TrackingWasInterrupted,
        };

    public static implicit operator ContentEncounter(ContentEncounterDto c)
        => new()
        {
            Id = c.Id,
            ContentRouletteId = c.ContentRouletteId,
            TerritoryTypeId = c.TerritoryTypeId,
            PlayerContentId = c.PlayerContentId,
            ClassJobId = c.ClassJobId,
            Completed = c.Completed,
            StartDate = DateTimeOffset.FromUnixTimeMilliseconds(c.StartDate),
            EndDate = DateTimeOffset.FromUnixTimeMilliseconds(c.EndDate),
            ActiveContentCondition = (c.JoinInProgressEnabled ? ContentCondition.JoinInProgress : 0) |
                                     (c.UnrestrictedParty ? ContentCondition.UnrestrictedParty : 0) |
                                     (c.MinimalIL ? ContentCondition.MinimalIL : 0) |
                                     (c.LevelSync ? ContentCondition.LevelSync : 0) |
                                     (c.SilenceEcho ? ContentCondition.SilenceEcho : 0) |
                                     (c.ExplorerMode ? ContentCondition.ExplorerMode : 0),
            JoinInProgress = c.JoinInProgress,
            QueuePlayerCount = c.QueuePlayerCount,
            Wipes = c.Wipes,
            PlayerDeathCount = c.PlayerDeathCount,
            TrackingWasInterrupted = c.Interrupted,
        };
}