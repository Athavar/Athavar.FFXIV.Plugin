// <copyright file="DutyHistoryTable.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using Athavar.FFXIV.Plugin.Data;
using Athavar.FFXIV.Plugin.Models.Data;
using Athavar.FFXIV.Plugin.Models.Types;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Table;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

public sealed class DutyHistoryTable : Table<ContentEncounter>, IDisposable
{
    private static readonly ContentFinderNameColumn ContentFinderNameColumnValue = new() { Label = "ContentName" };
    private static readonly TerritoryTypeColumn TerritoryTypeColumnValue = new() { Label = "Territory" };
    private static readonly ContentRouletteColumn ContentRouletteColumnValue = new() { Label = "Roulette" };
    private static readonly StartDateColumn StartDateColumnValue = new() { Label = "StartTime" };
    private static readonly DurationColumn DurationColumnValue = new() { Label = "Duration" };
    private static readonly CompleteColumn CompleteColumnValue = new() { Label = "Complete" };
    private static readonly JoinInProgressColumn JoinInProgressColumnValue = new() { Label = "JoinInProgress" };

    private static float globalScale;
    private static float itemSpacingX;
    private static float territoryColumnWidth;
    private static float contentFinderNameColumnWidth;
    private static float rouletteColumnWidth;
    private static float startTimeColumnWidth;
    private static float durationTimeColumnWidth;
    private static float completeColumnWidth;
    private static float joinInProgressColumnWidth;

    private readonly RepositoryContext context;
    private readonly IDataManager dataManager;
    private readonly StateTracker stateTracker;

    public DutyHistoryTable(RepositoryContext context, StateTracker stateTracker, IDataManager dataManager)
        : base("dutyTrackerTable", new List<ContentEncounter>(), ContentRouletteColumnValue, ContentFinderNameColumnValue, StartDateColumnValue, DurationColumnValue, CompleteColumnValue, JoinInProgressColumnValue)
    {
        this.context = context;
        this.stateTracker = stateTracker;
        this.dataManager = dataManager;
        this.stateTracker.NewContentEncounter += this.OnNewContentEncounter;
    }

    public void Dispose() => this.stateTracker.NewContentEncounter -= this.OnNewContentEncounter;

    public void Update(ulong playerId)
        => Task.Run(() =>
        {
            if (this.Items is List<ContentEncounter> list)
            {
                list.Clear();
                list.AddRange(this.context.ContentEncounter.GetAllContentEncounterForPlayerId(playerId));
            }

            this.filterDirty = true;
            this.sortDirty = true;
        });

    protected override void PreDraw()
    {
        if (Math.Abs(globalScale - ImGuiHelpers.GlobalScale) > 0.00001)
        {
            globalScale = ImGuiHelpers.GlobalScale;
            itemSpacingX = ImGui.GetStyle().ItemSpacing.X;
            territoryColumnWidth = (this.dataManager.GetExcelSheet<TerritoryType>()!.Max(tt => ImGui.CalcTextSize(tt.Name).X) + (itemSpacingX * 2)) / globalScale;
            contentFinderNameColumnWidth = (this.dataManager.GetExcelSheet<ContentFinderCondition>()!.Max(tt => ImGui.CalcTextSize(tt.Name).X) + (itemSpacingX * 2)) / globalScale;
            rouletteColumnWidth = (this.dataManager.GetExcelSheet<ContentRoulette>()!.Max(tt => ImGui.CalcTextSize(tt.Name).X) + (itemSpacingX * 2)) / globalScale;
            startTimeColumnWidth = (ImGui.CalcTextSize("000-00-00T00:00:00").X + (itemSpacingX * 2)) / globalScale;
            durationTimeColumnWidth = (ImGui.CalcTextSize("0:00:00.000").X + (itemSpacingX * 2)) / globalScale;
            completeColumnWidth = (ImGui.CalcTextSize(CompleteColumnValue.Label).X + (itemSpacingX * 2)) / globalScale;
            completeColumnWidth = (ImGui.CalcTextSize(JoinInProgressColumnValue.Label).X + (itemSpacingX * 2)) / globalScale;
        }
    }

    private void OnNewContentEncounter(ContentEncounter encounter)
    {
        this.Items.Add(encounter);
        this.filterDirty = true;
        this.sortDirty = true;
    }

    private sealed class ContentFinderNameColumn : ColumnString<ContentEncounter>
    {
        public override float Width => contentFinderNameColumnWidth * ImGuiHelpers.GlobalScale;

        public override string ToName(ContentEncounter item) => item.ContentFinderCondition?.Name.ToString() ?? "---";
    }

    private sealed class TerritoryTypeColumn : ColumnString<ContentEncounter>
    {
        public override float Width => territoryColumnWidth * ImGuiHelpers.GlobalScale;

        public override string ToName(ContentEncounter item) => item.TerritoryType?.Name.ToString() ?? item.TerritoryTypeId.ToString();
    }

    private sealed class ContentRouletteColumn : ColumnString<ContentEncounter>
    {
        public override float Width => rouletteColumnWidth * ImGuiHelpers.GlobalScale;

        public override string ToName(ContentEncounter item) => item.ContentRoulette?.Name.ToString() ?? "---";
    }

    private sealed class StartDateColumn : ColumnString<ContentEncounter>
    {
        public override float Width => startTimeColumnWidth * ImGuiHelpers.GlobalScale;

        public override string ToName(ContentEncounter item) => item.StartDate.ToString("s");

        public override int Compare(ContentEncounter lhs, ContentEncounter rhs) => lhs.StartDate.CompareTo(rhs.StartDate);
    }

    private sealed class DurationColumn : ColumnString<ContentEncounter>
    {
        public override float Width => durationTimeColumnWidth * ImGuiHelpers.GlobalScale;

        public override string ToName(ContentEncounter item) => item.Duration.ToString(@"h\:mm\:ss\.FF");

        public override int Compare(ContentEncounter lhs, ContentEncounter rhs) => lhs.Duration.CompareTo(rhs.Duration);
    }

    private sealed class CompleteColumn : ColumnBool<ContentEncounter>
    {
        public override float Width => completeColumnWidth * ImGuiHelpers.GlobalScale;

        public override bool ToBool(ContentEncounter item) => item.Completed;
    }

    private sealed class JoinInProgressColumn : ColumnBool<ContentEncounter>
    {
        public override float Width => completeColumnWidth * ImGuiHelpers.GlobalScale;

        public override bool ToBool(ContentEncounter item) => item.JoinInProgress;
    }
}