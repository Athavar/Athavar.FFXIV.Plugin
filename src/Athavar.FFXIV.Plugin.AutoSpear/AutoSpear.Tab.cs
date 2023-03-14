// <copyright file="AutoSpear.Tab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using ImGuiNET;

internal sealed partial class AutoSpear
{
    private readonly string[] fishData = new string[3] { string.Empty, string.Empty, string.Empty };

    public override string Name => "AutoSpear";

    public override string Identifier => "autospear";

    public override void Draw()
    {
        var text = this.configuration.FishMatchText ??= string.Empty;
        if (ImGui.InputText("Spear Fish Name Matcher", ref text, 1024))
        {
            this.configuration.FishMatchText = text;
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