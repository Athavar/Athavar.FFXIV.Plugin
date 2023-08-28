// <copyright file="StateTracker.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Manager.Models;
using Athavar.FFXIV.Plugin.Data;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Data;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Plugin.Services;

public sealed class StateTracker : IDisposable
{
    private readonly IPluginLogger pluginLogger;
    private readonly IClientState clientState;
    private readonly IDutyManager dutyManager;
    private readonly IChatManager chatManager;
    private readonly RepositoryContext repositoryContext;

    private uint? classJobId = 0;

    public StateTracker(IPluginLogger pluginLogger, IClientState clientState, IDutyManager dutyManager, IChatManager chatManager, RepositoryContext repositoryContext)
    {
        this.pluginLogger = pluginLogger;
        this.clientState = clientState;
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
            ContentFinderCondition = args.DutyInfo.TerritoryType.ContentFinderCondition.Value,
            /* Known case where classJob is null/0: JoinInProgress + NotCompleted. TODO: Fix this */
            ClassJobId = this.clientState.LocalPlayer?.ClassJob.Id ?? this.classJobId ?? 0,
            Completed = args.Completed,
            StartDate = args.StartTime,
            EndDate = args.EndTime,
            UnrestrictedParty = (args.DutyInfo.ActiveContentCondition & ContentCondition.UnrestrictedParty) != 0,
            MinimalIL = (args.DutyInfo.ActiveContentCondition & ContentCondition.MinimalIL) != 0,
            LevelSync = (args.DutyInfo.ActiveContentCondition & ContentCondition.LevelSync) != 0,
            SilenceEcho = (args.DutyInfo.ActiveContentCondition & ContentCondition.SilenceEcho) != 0,
            ExplorerMode = (args.DutyInfo.ActiveContentCondition & ContentCondition.ExplorerMode) != 0,
            JoinInProgress = args.DutyInfo.JoinInProgress,
            QueuePlayerCount = args.DutyInfo.QueuePlayerCount,
            Wipes = args.Wipes,
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

    private unsafe void OnDutyStarted(DutyStartedEventArgs args)
    {
        this.classJobId = this.clientState.LocalPlayer?.ClassJob.Id;
#if DEBUG
        this.chatManager.PrintChat($"[DT] Duty start: {args.DutyInfo.TerritoryType.RowId}|{args.DutyInfo.TerritoryType.TerritoryIntendedUse}");
#endif
    }
}