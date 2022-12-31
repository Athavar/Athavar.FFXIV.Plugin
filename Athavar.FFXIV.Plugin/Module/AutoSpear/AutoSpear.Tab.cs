// <copyright file="AuroSpear.Tab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using Athavar.FFXIV.Plugin.Utils;
using ImGuiNET;

internal partial class AutoSpear
{
    private readonly string[] fishData = new string[3] { string.Empty, string.Empty, string.Empty };

    public void DrawTab()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem("AutoSpear"), ImGui.EndTabItem))
        {
            return;
        }

        this.configuration.FishMatchText ??= string.Empty;
        if (ImGui.InputText("Spear Fish Name Matcher", ref this.configuration.FishMatchText, 1024))
        {
            this.configuration.Save();
        }

        if (ImGui.CollapsingHeader("Debug"))
        {
            foreach (var line in this.fishData)
            {
                ImGui.Text(line);
            }
        }
    }
}