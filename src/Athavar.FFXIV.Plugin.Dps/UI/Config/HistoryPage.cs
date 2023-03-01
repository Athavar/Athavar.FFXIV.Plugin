// <copyright file="HistoryPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Globalization;
using System.Numerics;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Dalamud.Interface.Colors;
using ImGuiNET;

internal class HistoryPage : IConfigPage
{
    private static readonly ImGuiTableFlags flags = ImGuiTableFlags.Sortable | ImGuiTableFlags.ScrollY |
                                                    ImGuiTableFlags.BordersV | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.Resizable | ImGuiTableFlags.RowBg | ImGuiTableFlags.NoBordersInBody;

    private static readonly string[] ColumnNames = { "Name", "Duration", "Dps" };

    private readonly EncounterManager encounterManager;
    private readonly Utils utils;

    private Columns sortColumn;
    private ImGuiSortDirection sortDirection;

    public HistoryPage(EncounterManager encounterManager, Utils utils)
    {
        this.encounterManager = encounterManager;
        this.utils = utils;
    }

    private enum Columns
    {
        Name,
        Duration,
        Dps,
        Hps,
        Kills,
        Deaths,
    }

    public string Name => "History";

    public IConfig GetDefault() => new DummyConfig();

    public IConfig GetConfig() => new DummyConfig();

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        if (ImGui.BeginTable("Encounter", 6, flags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.NoSort, 150f);
            ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.NoSort, 10 * 12.0f);
            ImGui.TableSetupColumn("Dps", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.PreferSortDescending, 8 * 18.0f);
            ImGui.TableSetupColumn("Hps", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.PreferSortDescending, 8 * 18.0f);
            ImGui.TableSetupColumn("Kills", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.PreferSortDescending, 5 * 18.0f);
            ImGui.TableSetupColumn("Deaths", ImGuiTableColumnFlags.WidthStretch | ImGuiTableColumnFlags.PreferSortDescending, 5 * 18.0f);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            var pts = ImGui.TableGetSortSpecs();
            if (pts.SpecsDirty)
            {
                this.sortDirection = pts.Specs.SortDirection;
                this.sortColumn = (Columns)pts.Specs.ColumnIndex;
                pts.SpecsDirty = false;
            }

            this.DrawEncounter(this.encounterManager.CurrentTerritoryEncounter);
            for (var index = 0; index < this.encounterManager.EncounterHistory.Count; index++)
            {
                var encounter = this.encounterManager.EncounterHistory[index];
                if (encounter == this.encounterManager.CurrentTerritoryEncounter)
                {
                    continue;
                }

                this.DrawEncounter(encounter);
            }

            ImGui.EndTable();
        }
    }

    private Func<BaseCombatant, double> GetSortKey()
    {
        double mod = this.sortDirection == ImGuiSortDirection.Descending ? -1 : 1;
        return this.sortColumn switch
               {
                   Columns.Dps => combatant => combatant.Dps * mod,
                   Columns.Hps => combatant => combatant.Hps * mod,
                   Columns.Kills => combatant => combatant.Kills * mod,
                   Columns.Deaths => combatant => combatant.Deaths * mod,
                   _ => _ => 0,
               };
    }

    private void DrawEncounter(BaseEncounter? encounter)
    {
        void DrawTerritoryCombatants(TerritoryEncounter e)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            if (ImGui.TreeNodeEx("Combatants##combatants", ImGuiTreeNodeFlags.SpanFullWidth))
            {
                DrawCombatants(e);
                ImGui.TreePop();
            }
        }

        void DrawCombatants(BaseEncounter e)
        {
            var combatants = e.GetCombatants().Where(c => c.IsActive()).OrderBy(this.GetSortKey()).ToList();
            foreach (var t in combatants)
            {
                this.DrawBaseCombatant(e, t);
            }
        }

        if (encounter is null)
        {
            return;
        }

        if (this.DrawBaseEncounter(encounter))
        {
            if (encounter is TerritoryEncounter te)
            {
                DrawTerritoryCombatants(te);

                for (var index = 0; index < te.Encounters.Count; index++)
                {
                    this.DrawEncounter(te.Encounters[index]);
                }
            }
            else
            {
                DrawCombatants(encounter);
            }

            ImGui.TreePop();
        }
    }

    private bool DrawBaseEncounter(BaseEncounter encounter)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        var open = ImGui.TreeNodeEx($"{encounter.TitleStart}", ImGuiTreeNodeFlags.SpanFullWidth);
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(encounter.Duration.ToString("hh\\:mm\\:ss"));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(encounter.Dps.ToString(CultureInfo.InvariantCulture));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(encounter.Hps.ToString(CultureInfo.InvariantCulture));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(encounter.Kills.ToString(CultureInfo.InvariantCulture));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(encounter.Deaths.ToString(CultureInfo.InvariantCulture));

        return open;
    }

    private void DrawBaseCombatant(BaseEncounter encounter, BaseCombatant combatant)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        var colorPushed = true;
        if (combatant.IsEnemy())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        }
        else if (combatant.PartyType > encounter.Filter)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        }
        else
        {
            colorPushed = false;
        }

        ImGui.TreeNodeEx(combatant.Name, ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.Bullet | ImGuiTreeNodeFlags.NoTreePushOnOpen | ImGuiTreeNodeFlags.SpanFullWidth);
        this.utils.DrawActionSummaryTooltip(combatant);

        ImGui.TableNextColumn();
        ImGui.TextDisabled("--");
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(combatant.Dps.ToString(CultureInfo.InvariantCulture));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(combatant.Hps.ToString(CultureInfo.InvariantCulture));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(combatant.Kills.ToString(CultureInfo.InvariantCulture));
        ImGui.TableNextColumn();
        ImGui.TextUnformatted(combatant.Deaths.ToString(CultureInfo.InvariantCulture));
        if (colorPushed)
        {
            ImGui.PopStyleColor();
        }
    }
}