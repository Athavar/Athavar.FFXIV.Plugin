// <copyright file="GeneralConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using ImGuiNET;

internal class GeneralConfigPage : IConfigPage
{
    [JsonIgnore]
    public bool Preview;

    [JsonIgnore]
    private static readonly string[] MeterTypeOptions = Enum.GetNames(typeof(MeterDataType));

    private readonly MeterConfig config;

    public GeneralConfigPage(MeterConfig config) => this.config = config;

    public string Name => "General";

    private GeneralConfig Config => this.config.GeneralConfig;

    public IConfig GetDefault() => new GeneralConfig();

    public IConfig GetConfig() => this.Config;

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            var screenSize = ImGui.GetMainViewport().Size;
            var c = this.Config;
            ImGuiEx.DragFloat2("Position", c.Position, x => c.Position = x, 1, -screenSize.X / 2, screenSize.X / 2);
            ImGuiEx.DragFloat2("Size", c.Size, x => c.Size = x, 1, 0, screenSize.Y);
            ImGuiEx.Checkbox("Lock", c.Lock, x => c.Lock = x);
            ImGuiEx.Checkbox("Click Through", c.ClickThrough, x => c.ClickThrough = x);
            ImGuiEx.Checkbox("Preview", c.Preview, x => c.Preview = x);

            ImGui.NewLine();

            ImGuiEx.ColorEdit4("Background Color", c.BackgroundColor.Vector, x => c.BackgroundColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);

            ImGuiEx.Checkbox("Show Border", c.ShowBorder, x => c.ShowBorder = x);
            if (c.ShowBorder)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.DragInt("Border Thickness", c.BorderThickness, x => c.BorderThickness = x, 1, 1, 20);

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Border Color", c.BorderColor.Vector, x => c.BorderColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.Checkbox("Hide border around Header", c.BorderAroundBars, x => c.BorderAroundBars = x);
            }

            ImGui.NewLine();
            ImGuiEx.Combo("Sort Type", c.DataType, x => c.DataType = x, MeterTypeOptions);

            ImGuiEx.Checkbox("Return to Current Data when entering combat", c.ReturnToCurrent, x => c.ReturnToCurrent = x);
        }

        ImGui.EndChild();
    }
}