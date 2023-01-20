// <copyright file="CraftQueueTab.Stats.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.UI;
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
            if (!ImGui.BeginTable("##gearset-table", 7))
            {
                return;
            }

            ImGui.TableSetupColumn("Id", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Gearset", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Craftsmanship", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Control", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("CP", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Specialist", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var sheet = this.dataManager.GetExcelSheet<ClassJob>()!;
            foreach (var gearset in this.gearsetManager.AllGearsets.Where(g => g.GetCraftingJob() is not null))
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted($"{gearset.Id}");
                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(gearset.Name);
                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(sheet?.GetRow((uint)gearset.JobClass)?.Abbreviation?.RawString ?? "???");
                ImGui.TableSetColumnIndex(3);
                ImGui.TextUnformatted($"{gearset.Craftsmanship}");
                ImGui.TableSetColumnIndex(4);
                ImGui.TextUnformatted($"{gearset.Control}");
                ImGui.TableSetColumnIndex(5);
                ImGui.TextUnformatted($"{gearset.CP}");
                ImGui.TableSetColumnIndex(6);
                ImGui.TextUnformatted($"{gearset.HasSoulStone}");
            }

            ImGui.EndTable();
        }

        DrawCraftingGearsetTable();
    }
}