// <copyright file="HuntLinkTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.HuntLink
{
    using System.Collections.Generic;
    using System.Linq;

    using ImGuiNET;
    using Lumina.Excel.GeneratedSheets;

    internal class HuntLinkTab
    {
        // ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1)

        private List<World> worlds;
        private string[] worldNames;
        private List<Fate> fates;

        private int selectWorld = 0;

        public HuntLinkTab()
        {
            this.PopulateWorlds();
            this.PopulateFates();
        }

        public void DrawTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("HuntLink"), ImGui.EndTabItem))
            {
                return;
            }

            if (ImGui.ListBox("World", ref this.selectWorld, this.worldNames, this.worldNames.Length))
            {

            }

            foreach (var fate in this.fates)
            {
                fate.Location.ToString();
                ImGui.Text($"{fate.RowId}: {fate.ClassJobLevel} - {fate.Name} - {fate.Location}");
            }
        }

        private void PopulateWorlds()
        {
            var dc = DalamudBinding.ClientState.LocalPlayer?.HomeWorld.GameData.DataCenter.Row;

            if (dc == null)
            {
                return;
            }

            this.worlds = DalamudBinding.DataManager.GetExcelSheet<World>()!.Where(w => w.DataCenter.Row == dc).ToList();
            this.worldNames = this.worlds.Select(w => w.Name.RawString).ToArray();
        }

        private void PopulateFates()
        {
            this.fates = DalamudBinding.DataManager.GetExcelSheet<Fate>()!.Where(w => w.Unknown24 == 1).ToList();
        }
    }
}
