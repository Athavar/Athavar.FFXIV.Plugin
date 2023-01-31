// <copyright file="ConfigTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.UI;
using ImGuiNET;

internal class ConfigTab : Tab
{
    private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    public ConfigTab(CraftQueueConfiguration configuration) => this.Configuration = configuration;

    /// <inheritdoc />
    public override string Name => "Config";

    /// <inheritdoc />
    public override string Identifier => "config";

    private CraftQueueConfiguration Configuration { get; }

    /// <inheritdoc />
    public override void Draw()
    {
        void DisplayOption(params string[] lines)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, this.shadedColor);

            foreach (var line in lines)
            {
                ImGui.TextWrapped(line);
            }

            ImGui.PopStyleColor();
        }

        var craftWaitSkip = this.Configuration.CraftWaitSkip;
        if (ImGui.Checkbox("No Craft Wait", ref craftWaitSkip))
        {
            this.Configuration.CraftWaitSkip = craftWaitSkip;
            this.Configuration.Save();
        }

        DisplayOption("- Don't wait after a craft commands like normal in-game macros.");

        ImGui.Separator();
        var qualitySkip = this.Configuration.QualitySkip;
        if (ImGui.Checkbox("Quality Skip", ref qualitySkip))
        {
            this.Configuration.QualitySkip = qualitySkip;
            this.Configuration.Save();
        }

        DisplayOption("- Skip quality increasing actions when the HQ chance is at 100%. If you depend on durability increases from Manipulation towards the end of your macro, you will likely want to disable this.");
    }
}