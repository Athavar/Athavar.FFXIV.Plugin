// <copyright file="HuntLinkTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.HuntLink;

using Athavar.FFXIV.Plugin.Utils;
using ImGuiNET;

internal class HuntLinkTab
{
    private readonly HuntLinkDatastore datastore;

    public HuntLinkTab(HuntLinkDatastore datastore) => this.datastore = datastore;

    public void DrawTab()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem("HuntLink"), ImGui.EndTabItem))
        {
            return;
        }

        ImGui.Columns(2);
        if (ImGui.BeginCombo("World", this.datastore.SelectedWorld?.Name.RawString ?? "World..."))
        {
            foreach (var world in this.datastore.Worlds)
            {
                var selected = world.RowId == this.datastore.SelectedWorld?.RowId;
                var text = world.Name.RawString;
                if (ImGui.Selectable(text, ref selected))
                {
                    this.datastore.SelectedWorld = world;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.NextColumn();

        if (ImGui.BeginTable("Fates", 4))
        {
            foreach (var fate in this.datastore.Fates)
            {
                fate.Location.ToString();
                ImGui.TableNextRow();
                ImGui.Text(fate.RowId + string.Empty);
                ImGui.TableNextColumn();
                ImGui.Text(fate.ClassJobLevel + string.Empty);
                ImGui.TableNextColumn();
                ImGui.Text(fate.Name);
                ImGui.TableNextColumn();
                ImGui.Text(fate.Location + string.Empty);
            }

            ImGui.EndTable();
        }
    }
}