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

    private readonly MeterWindow window;

    public GeneralConfigPage(MeterWindow window) => this.window = window;

    public string Name => "General";

    private GeneralConfig Config => this.window.Config.GeneralConfig;

    public IConfig GetDefault() => new GeneralConfig();

    public IConfig GetConfig() => this.Config;

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            var change = false;
            var screenSize = ImGui.GetMainViewport().Size;
            var c = this.Config;
            if (ImGuiEx.DragFloat2("Position", c.Position, x => c.Position = x, 1, -screenSize.X / 2, screenSize.X / 2))
            {
                change = true;
            }

            if (ImGuiEx.DragFloat2("Size", c.Size, x => c.Size = x, 1, 0, screenSize.Y))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Lock", c.Lock, x => c.Lock = x))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Click Through", c.ClickThrough, x => c.ClickThrough = x))
            {
                change = true;
            }
            // ImGuiEx.Checkbox("Preview", c.Preview, x => c.Preview = x);

            ImGui.NewLine();

            if (ImGuiEx.ColorEdit4("Background Color", c.BackgroundColor.Vector, x => c.BackgroundColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Show Border", c.ShowBorder, x => c.ShowBorder = x))
            {
                change = true;
            }

            if (c.ShowBorder)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.DragInt("Border Thickness", c.BorderThickness, x => c.BorderThickness = x, 1, 1, 20))
                {
                    change = true;
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.ColorEdit4("Border Color", c.BorderColor.Vector, x => c.BorderColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                {
                    change = true;
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Checkbox("Hide border around Header", c.BorderAroundBars, x => c.BorderAroundBars = x))
                {
                    change = true;
                }
            }

            ImGui.NewLine();
            if (ImGuiEx.Combo("Sort Type", c.DataType, x => c.DataType = x, MeterTypeOptions))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Return to Current Data when entering combat", c.ReturnToCurrent, x => c.ReturnToCurrent = x))
            {
                change = true;
            }

            // Save if changed
            if (change)
            {
                this.window.Save();
            }

            ImGui.EndChild();
        }
    }
}