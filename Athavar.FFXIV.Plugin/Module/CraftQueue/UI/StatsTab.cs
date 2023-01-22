// <copyright file="CraftQueueTab.Stats.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.UI;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Data;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

internal class StatsTab : Tab
{
    private readonly IGearsetManager gearsetManager;
    private readonly DataManager dataManager;

    public StatsTab(IGearsetManager gearsetManager, DataManager dataManager)
    {
        this.gearsetManager = gearsetManager;
        this.dataManager = dataManager;
    }

    /// <inheritdoc />
    protected internal override string Name => "Stats";

    /// <inheritdoc />
    protected internal override string Identifier => "Tab-CQStatsOptions";

    /// <inheritdoc />
    public override void Draw()
    {
        if (ImGui.Button("Refresh"))
        {
            this.gearsetManager.UpdateGearsets();
        }

        void DrawCraftingGearsetTable()
        {
            ImGui.TextUnformatted("Gearsets need to be saved with all Materia for the correct detection.");
            if (!ImGui.BeginTable("##gearset-table", 8))
            {
                return;
            }

            ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Gearset", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Level", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Craftsmanship", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Control", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("CP", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Specialist", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var sheet = this.dataManager.GetExcelSheet<ClassJob>()!;
            foreach (var gearset in this.gearsetManager.AllGearsets.Where(g => g.GetCraftingJob() is not null))
            {
                var columns = new object[8];
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                columns[0] = gearset.Id;
                columns[1] = gearset.Name;
                columns[2] = sheet?.GetRow((uint)gearset.JobClass)?.Abbreviation?.RawString ?? "???";
                columns[3] = gearset.JobLevel;
                columns[4] = gearset.Craftsmanship;
                columns[5] = gearset.Control;
                columns[6] = gearset.CP;
                columns[7] = gearset.HasSoulStone;
                ImGuiEx.TableRow(columns);
            }

            ImGui.EndTable();
        }

        DrawCraftingGearsetTable();
    }
}