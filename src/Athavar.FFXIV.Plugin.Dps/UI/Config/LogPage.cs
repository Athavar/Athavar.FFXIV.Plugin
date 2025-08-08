// <copyright file="LogPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Dalamud.Bindings.ImGui;

internal sealed class LogPage : IConfigPage
{
    private readonly EncounterManager encounterManager;
    private readonly NetworkHandler networkHandler;

    public LogPage(EncounterManager encounterManager, NetworkHandler networkHandler)
    {
        this.encounterManager = encounterManager;
        this.networkHandler = networkHandler;
    }

    public string Name => "Log";

    public IConfig GetDefault() => new DummyConfig();

    public IConfig GetConfig() => new DummyConfig();

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        this.DrawDpsTable();
        ImGui.Separator();
        this.DrawLogs();
    }

    private void DrawHistory()
    {
        void DrawEncounter(BaseEncounter? encounter, int level = 1)
        {
            if (encounter is null || !encounter.IsValid())
            {
                return;
            }

            var ally = encounter.GetAllyCombatants().ToList();
            ImGuiEx.DrawNestIndicator(level);
            ImGui.SameLine();
            ImGui.TextUnformatted($"{encounter.Start:t} {encounter.Duration:t}  {encounter.Title} Ally:{ally.Count} Combatant:{encounter.GetCombatants().Count()}");
            foreach (var combatant in ally)
            {
                ImGuiEx.DrawNestIndicator(level + 1);
                ImGui.SameLine();
                ImGui.TextUnformatted($"{combatant.Name} - {combatant.DamageTotal}");
            }

            if (encounter is TerritoryEncounter territoryEncounter)
            {
                ImGuiEx.DrawNestIndicator(level);
                ImGui.SameLine();
                ImGui.TextUnformatted("Encounter:");
                foreach (var territoryEncounterEncounter in territoryEncounter.Encounters)
                {
                    DrawEncounter(territoryEncounterEncounter, level + 1);
                }
            }
        }

        ImGui.TextUnformatted("Current Encounter:");
        DrawEncounter(this.encounterManager.CurrentEncounter);

        ImGui.TextUnformatted("Current Territory:");
        DrawEncounter(this.encounterManager.CurrentTerritoryEncounter);
    }

    private void DrawDpsTable()
    {
        if (ImGui.Button("Clear"))
        {
            this.encounterManager.Log.Clear();
        }

        ImGui.SameLine();
        ImGuiEx.Checkbox("NetworkDebug", this.networkHandler.Debug, x => this.networkHandler.Debug = x);

        var encounter = this.encounterManager.GetEncounter();

        if (encounter is not null && ImGui.BeginTable("##combatents", 8))
        {
            ImGui.TableSetupColumn("Name##name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Level##level", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Dps##dps", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("DamageTotal##damageTotal", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Hps##hps", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("HealingTotal##healingTotal", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("OverHeal##overHeal", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("DamageTaken##damageTaken", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            foreach (var combatant in encounter.GetAllyCombatants())
            {
                ImGui.TableNextRow();
                var row = new object[8];
                row[0] = combatant.Name;
                row[1] = combatant.Level;
                row[2] = combatant.Dps;
                row[3] = combatant.DamageTotal;
                row[4] = combatant.Hps;
                row[5] = combatant.HealingTotal;
                row[6] = combatant.OverHealPct + "%";
                row[7] = combatant.DamageTaken;
                ImGuiEx.TableRow(row);
            }

            ImGui.EndTable();
        }
    }

    private void DrawLogs()
    {
        if (ImGui.BeginChild("loglist"))
        {
            foreach (var line in this.encounterManager.Log)
            {
                ImGui.TextUnformatted(line);
            }

            ImGui.EndChild();
        }
    }
}