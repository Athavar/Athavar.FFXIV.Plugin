// <copyright file="DutyHistoryTable.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.DutyHistory;

using System.Runtime.CompilerServices;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Data;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Data;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Athavar.FFXIV.Plugin.Models.Types;
using Dalamud.Game;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Table;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel.Sheets;

public sealed class DutyHistoryTable : Table<ContentEncounter>, IDisposable
{
    private static readonly ContentFinderNameColumn ContentFinderNameColumnValue = new() { Label = "ContentName" };
    private static readonly TerritoryTypeColumn TerritoryTypeColumnValue = new() { Label = "Territory" };
    private static readonly ContentRouletteColumn ContentRouletteColumnValue = new() { Label = "Roulette" };
    private static readonly StartDateColumn StartDateColumnValue = new() { Label = "StartTime" };
    private static readonly DurationColumn DurationColumnValue = new() { Label = "Duration" };
    private static readonly CompleteColumn CompleteColumnValue = new() { Label = "C", Tooltip = "Complete" };
    private static readonly JoinInProgressColumn JoinInProgressColumnValue = new() { Label = "JIP", Tooltip = "JoinInProgress" };
    private static readonly ConditionColumn ConditionColumnValue = new() { Label = "DutySettings" };
    private static readonly ClassJobColumn ClassJobColumnValue = new() { Label = "Job" };

    private static float globalScale;
    private static float itemSpacingX;
    private static float territoryColumnWidth;
    private static float contentFinderNameColumnWidth;
    private static float rouletteColumnWidth;
    private static float startTimeColumnWidth;
    private static float durationTimeColumnWidth;
    private static float completeColumnWidth;
    private static float joinInProgressColumnWidth;
    private static float conditionColumnWidth;
    private static float classJobColumnWidth;

    private readonly RepositoryContext context;
    private readonly IDataManager dataManager;
    private readonly StateTracker stateTracker;

    public DutyHistoryTable(RepositoryContext context, StateTracker stateTracker, IDataManager dataManager, IIconManager iconManager)
        : base("dutyTrackerTable", new List<ContentEncounter>(), ContentFinderNameColumnValue, ContentRouletteColumnValue, StartDateColumnValue, DurationColumnValue, ClassJobColumnValue, CompleteColumnValue, ConditionColumnValue, JoinInProgressColumnValue)
    {
        this.context = context;
        this.stateTracker = stateTracker;
        this.dataManager = dataManager;
        this.stateTracker.NewContentEncounter += this.OnNewContentEncounter;
        ClassJobColumnValue.Init(dataManager, iconManager);
        ConditionColumnValue.Init(dataManager, iconManager);
        this.Flags |= ImGuiTableFlags.SizingFixedFit;

        ContentFinderNameColumnValue.Language = dataManager.Language;
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
            territoryColumnWidth = (this.dataManager.GetExcelSheet<TerritoryType>().Max(tt => ImGui.CalcTextSize(tt.Name.ToString()).X) + (itemSpacingX * 2)) / globalScale;
            contentFinderNameColumnWidth = (this.dataManager.GetExcelSheet<ContentFinderCondition>().Max(tt => ImGui.CalcTextSize(tt.Name.ToString()).X) + (itemSpacingX * 2)) / globalScale;
            rouletteColumnWidth = (this.dataManager.GetExcelSheet<ContentRoulette>().Max(tt => ImGui.CalcTextSize(tt.Name.ToString()).X) + (itemSpacingX * 2)) / globalScale;
            startTimeColumnWidth = (ImGui.CalcTextSize("000-00-00T00:00:00").X + (itemSpacingX * 2)) / globalScale;
            durationTimeColumnWidth = (ImGui.CalcTextSize("0:00:00.000").X + (itemSpacingX * 2)) / globalScale;
            completeColumnWidth = (ImGui.CalcTextSize(CompleteColumnValue.Label).X + (itemSpacingX * 2)) / globalScale;
            joinInProgressColumnWidth = (ImGui.CalcTextSize(JoinInProgressColumnValue.Label).X + (itemSpacingX * 2)) / globalScale;
            conditionColumnWidth = (ImGui.CalcTextSize(ConditionColumnValue.Label).X + (itemSpacingX * 2)) / globalScale;
            classJobColumnWidth = (ImGui.CalcTextSize(ClassJobColumnValue.Label).X + (itemSpacingX * 2)) / globalScale;
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

        public ClientLanguage Language { get; set; }

        public override string ToName(ContentEncounter item)
        {
            var name = item.ContentFinderCondition?[this.Language];
            return !string.IsNullOrWhiteSpace(name) ? name : $"Deleted [{item.TerritoryTypeId}]";
        }
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
        public override float Width => joinInProgressColumnWidth * ImGuiHelpers.GlobalScale;

        public override bool ToBool(ContentEncounter item) => item.JoinInProgress;
    }

    private sealed class ConditionColumn : ColumnFlags<ContentCondition, ContentEncounter>
    {
        private string[] flagNames = Enum.GetValues<ContentCondition>()
           .Select(p => p.ToString())
           .ToArray();

        private ContentCondition currentFilter;

        private IIconManager? icons;

        public ConditionColumn() => this.AllFlags = Enum.GetValues<ContentCondition>().Aggregate((l, r) => l | r);

        public override float Width => conditionColumnWidth * ImGuiHelpers.GlobalScale;

        public override ContentCondition FilterValue => ~this.currentFilter;

        protected override string[] Names => this.flagNames;

        public void Init(IDataManager dataManager, IIconManager iconManager)
        {
            this.icons = iconManager;
            var sheet = dataManager.GetExcelSheet<Addon>();
            var names = new string[this.Values.Count];
            for (var index = 0; index < this.Values.Count; index++)
            {
                var flag = this.Values[index];
                names[index] = sheet?.GetRowOrDefault(GetTooltipId(flag))?.Text.ExtractText() ?? flag.ToString();
            }

            this.flagNames = names;
        }

        public override bool FilterFunc(ContentEncounter item)
        {
            if (item.ActiveContentCondition == 0)
            {
                // encounter has no condition. Filter out.
                return this.FilterValue.HasFlag(ContentCondition.SelectNone);
            }

            return this.FilterValue.HasFlag(item.ActiveContentCondition);
        }

        public override int Compare(ContentEncounter lhs, ContentEncounter rhs) => lhs.ActiveContentCondition.CompareTo(rhs.ActiveContentCondition);

        public override void DrawColumn(ContentEncounter item, int i)
        {
            var first = false;
            var index = 0;
            foreach (var flag in this.Values)
            {
                if ((item.ActiveContentCondition & flag) != 0 && this.icons is not null && this.icons.TryGetIcon(GetIconId(flag), out var sharedImmediateTexture))
                {
                    var textureWrap = sharedImmediateTexture.GetWrapOrEmpty();
                    if (first)
                    {
                        ImGui.SameLine();
                    }
                    else
                    {
                        first = true;
                    }

                    ImGuiEx.ScaledImageY(textureWrap, ImGui.GetTextLineHeight());
                    ImGuiEx.TextTooltip(this.Names[index]);
                }

                index++;
            }
        }

        protected override void SetValue(ContentCondition value, bool enable)
        {
            var currentFilter = enable ? this.currentFilter & ~value : this.currentFilter | value;
            this.currentFilter = currentFilter;
        }

        private static uint GetIconId(ContentCondition condition)
            => condition switch
            {
                ContentCondition.JoinInProgress => 60644,
                ContentCondition.UnrestrictedParty => 60641,
                ContentCondition.MinimalIL => 60642,
                ContentCondition.LevelSync => 60649,
                ContentCondition.SilenceEcho => 60647,
                ContentCondition.ExplorerMode => 60648,
                ContentCondition.LimitedLevelingRoulette => 60640,
                _ => 0,
            };

        private static uint GetTooltipId(ContentCondition condition)
            => condition switch
            {
                ContentCondition.JoinInProgress => 2519,
                ContentCondition.UnrestrictedParty => 10008,
                ContentCondition.MinimalIL => 10010,
                ContentCondition.LevelSync => 12696,
                ContentCondition.SilenceEcho => 12691,
                ContentCondition.ExplorerMode => 13038,
                ContentCondition.LimitedLevelingRoulette => 13030,
                ContentCondition.SelectNone => 14727,
                _ => 0,
            };
    }

    private sealed class ClassJobColumn : ColumnFlags<JobFlag, ContentEncounter>
    {
        private string[] flagNames = Enum.GetValues<JobFlag>()
           .Select(p => p.ToString())
           .ToArray();

        private JobFlag currentFilter;

        private IIconManager? icons;

        public ClassJobColumn() => this.AllFlags = Enum.GetValues<JobFlag>().Aggregate((l, r) => l | r);

        public override float Width => classJobColumnWidth * ImGuiHelpers.GlobalScale;

        public override JobFlag FilterValue => ~this.currentFilter;

        protected override string[] Names => this.flagNames;

        public void Init(IDataManager dataManager, IIconManager iconManager)
        {
            this.icons = iconManager;
            var sheet = dataManager.GetExcelSheet<ClassJob>();
            var names = new string[this.Values.Count];
            for (var index = 0; index < this.Values.Count; index++)
            {
                var flag = this.Values[index];
                names[index] = sheet?.GetRowOrDefault((uint)index)?.Name.ExtractText() ?? flag.ToString();
            }

            this.flagNames = names;
        }

        public override bool FilterFunc(ContentEncounter item) => (this.FilterValue & ToFlag(item.ClassJobId)) != 0;

        public override int Compare(ContentEncounter lhs, ContentEncounter rhs) => lhs.ClassJobId.CompareTo(rhs.ClassJobId);

        public override void DrawColumn(ContentEncounter encounter, int i)
        {
            if (this.icons is not null && this.icons.TryGetJobIcon((Job)encounter.ClassJobId, JobIconStyle.Framed, true, out var sharedImmediateTexture))
            {
                var textureWrap = sharedImmediateTexture.GetWrapOrEmpty();
                ImGuiEx.ScaledCenterImageY(textureWrap, ImGui.GetTextLineHeight());
                ImGuiEx.TextTooltip(this.Names[encounter.ClassJobId]);
            }
        }

        protected override void SetValue(JobFlag value, bool enable)
        {
            var currentFilter = enable ? this.currentFilter & ~value : this.currentFilter | value;
            this.currentFilter = currentFilter;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static JobFlag ToFlag(uint jobId) => (JobFlag)(1 << (int)jobId);
    }
}