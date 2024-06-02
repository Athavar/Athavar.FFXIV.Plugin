// <copyright file="StatsTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;

internal sealed class StatsTab : Tab
{
    private readonly CraftQueue craftQueue;
    private readonly IDataManager dataManager;

    public StatsTab(CraftQueue craftQueue)
    {
        this.craftQueue = craftQueue;
        this.dataManager = craftQueue.DalamudServices.DataManager;
    }

    /// <inheritdoc/>
    public override string Name => "Stats";

    /// <inheritdoc/>
    public override string Identifier => "Tab-CQStatsOptions";

    /// <inheritdoc/>
    public override void Draw()
    {
        if (ImGui.Button("Refresh"))
        {
            this.craftQueue.GearsetManager.UpdateGearsets();
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
            foreach (var gearset in this.craftQueue.GearsetManager.AllGearsets.Where(g => g.GetCraftingJob() is not null))
            {
                var columns = new object[8];
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                columns[0] = gearset.Id + 1;
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