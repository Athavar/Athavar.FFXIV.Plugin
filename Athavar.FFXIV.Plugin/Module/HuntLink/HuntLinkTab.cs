// <copyright file="HuntLinkTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.HuntLink;

using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ImGuiNET;

internal class HuntLinkTab
{
    private readonly HuntLinkDatastore datastore;
    private readonly IChatManager chatManager;

    public HuntLinkTab(HuntLinkDatastore datastore, IChatManager chatManager) => (this.datastore, this.chatManager) = (datastore, chatManager);

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

        ImGui.Columns(1);
        if (ImGui.Button("SendTest"))
        {
            var trans = new AutoTranslatePayload(1, 101);
            var mes = new SeString().Append(trans);
            this.chatManager.SendMessage(mes);
        }
    }
}