// <copyright file="LogPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using ImGuiNET;

internal class LogPage : IConfigPage
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

            foreach (var combatant in encounter.AllyCombatants)
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