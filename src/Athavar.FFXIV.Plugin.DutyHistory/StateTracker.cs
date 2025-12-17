// <copyright file="StateTracker.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using Athavar.FFXIV.Plugin.Data;
using Athavar.FFXIV.Plugin.Models.Data;
using Athavar.FFXIV.Plugin.Models.Duty;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;
using IDefinitionManager = Athavar.FFXIV.Plugin.Common.Manager.Interface.IDefinitionManager;

public sealed class StateTracker : IDisposable
{
    private readonly IPluginLogger pluginLogger;
    private readonly IClientState clientState;
    private readonly IPlayerState playerState;
    private readonly IDefinitionManager definitionManager;
    private readonly IDutyManager dutyManager;
    private readonly IChatManager chatManager;
    private readonly RepositoryContext repositoryContext;

    private uint? classJobId = 0;

    public StateTracker(IPluginLogger pluginLogger, IClientState clientState, IPlayerState playerState, IDefinitionManager definitionManager, IDutyManager dutyManager, IChatManager chatManager, RepositoryContext repositoryContext)
    {
        this.pluginLogger = pluginLogger;
        this.clientState = clientState;
        this.playerState = playerState;
        this.definitionManager = definitionManager;
        this.dutyManager = dutyManager;
        this.chatManager = chatManager;
        this.repositoryContext = repositoryContext;
        dutyManager.DutyStarted += this.OnDutyStarted;
        dutyManager.DutyEnded += this.OnDutyCompleted;
    }

    public delegate void NewContentEncounterDelegate(ContentEncounter encounter);

    public event NewContentEncounterDelegate? NewContentEncounter;

    public void Dispose()
    {
        this.dutyManager.DutyStarted -= this.OnDutyStarted;
        this.dutyManager.DutyEnded -= this.OnDutyCompleted;
    }

    private void OnDutyCompleted(DutyEndedEventArgs args)
    {
#if DEBUG
        this.chatManager.PrintChat($"[DT] Duty end: Complete:{args.Completed}, {args.Duration:g}");
#endif
        var encounter = new ContentEncounter
        {
            ContentRouletteId = args.DutyInfo.ContentRoulette.RowId,
            ContentRoulette = args.DutyInfo.ContentRoulette.RowId > 0 ? args.DutyInfo.ContentRoulette : null,
            TerritoryTypeId = args.DutyInfo.TerritoryType.RowId,
            TerritoryType = args.DutyInfo.TerritoryType,
            ContentFinderCondition = this.definitionManager.GetContentName(args.DutyInfo.TerritoryType.RowId),
            PlayerContentId = this.playerState.ContentId,
            ClassJobId = this.playerState.ClassJob.RowId != 0 ? this.playerState.ClassJob.RowId : this.classJobId ?? 0,
            Completed = args.Completed,
            StartDate = args.StartTime,
            EndDate = args.EndTime,
            ActiveContentCondition = args.DutyInfo.ActiveContentCondition,
            JoinInProgress = args.DutyInfo.JoinInProgress,
            QueuePlayerCount = args.DutyInfo.QueuePlayerCount,
            Wipes = args.Wipes,
            PlayerDeathCount = args.PlayerDeaths,
            TrackingWasInterrupted = args.TrackingWasInterrupted,
        };

        this.NewContentEncounter?.Invoke(encounter);

        try
        {
            this.repositoryContext.ContentEncounter.AddContentEncounter(encounter);
        }
        catch (Exception ex)
        {
            this.pluginLogger.Error(ex, "Error while try to save encounter to db.");
        }
    }

    private void OnDutyStarted(DutyStartedEventArgs args)
    {
        this.classJobId = this.playerState.ClassJob.RowId;
#if DEBUG
        this.chatManager.PrintChat($"[DT] Duty start: {args.DutyInfo.TerritoryType.RowId}|{args.DutyInfo.TerritoryType.TerritoryIntendedUse.RowId}");
#endif
    }
}