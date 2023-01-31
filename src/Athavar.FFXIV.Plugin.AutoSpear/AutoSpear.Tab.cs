// <copyright file="AutoSpear.Tab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using ImGuiNET;

internal partial class AutoSpear
{
    private readonly string[] fishData = new string[3] { string.Empty, string.Empty, string.Empty };

    public override string Name => "AutoSpear";

    public override string Identifier => "autospear";

    public override void Draw()
    {
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