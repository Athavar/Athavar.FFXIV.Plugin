// <copyright file="HeaderConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data;
using ImGuiNET;

internal class HeaderConfigPage : IConfigPage
{
    [JsonIgnore]
    private static readonly string[] AnchorOptions = Enum.GetNames(typeof(DrawAnchor));

    private readonly MeterWindow window;
    private readonly IFontsManager fontsManager;

    public HeaderConfigPage(MeterWindow window, IFontsManager fontsManager)
    {
        this.window = window;
        this.fontsManager = fontsManager;
    }

    public string Name => "Header";

    private HeaderConfig Config => this.window.Config.HeaderConfig;

    public IConfig GetDefault()
    {
        var c = new HeaderConfig
        {
            DurationFontKey = FontsManager.DefaultSmallFontKey,
            DurationFontId = this.fontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
            NameFontKey = FontsManager.DefaultSmallFontKey,
            NameFontId = this.fontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
            StatsFontKey = FontsManager.DefaultSmallFontKey,
            StatsFontId = this.fontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
        };
        return c;
    }

    public IConfig GetConfig() => this.Config;

    public (Vector2, Vector2) DrawHeader(Vector2 pos, Vector2 size, BaseEncounter? encounter, ImDrawListPtr drawList)
    {
        if (!this.Config.ShowHeader)
        {
            return (pos, size);
        }

        var headerSize = new Vector2(size.X, this.Config.HeaderHeight);
        drawList.AddRectFilled(pos, pos + headerSize, this.Config.BackgroundColor.Base);

        var durationPos = pos;
        var durationSize = Vector2.Zero;
        if (this.Config.ShowEncounterDuration)
        {
            using (FontsManager.PushFont(this.Config.DurationFontKey))
            {
                var duration = encounter is null ? $" {DpsModule.ModuleName}" : $" {encounter.Duration:mm\\:ss\\.ff}";
                durationSize = ImGui.CalcTextSize(duration);
                durationPos = Utils.GetAnchoredPosition(durationPos, -headerSize, DrawAnchor.Left);
                durationPos = Utils.GetAnchoredPosition(durationPos, durationSize, this.Config.DurationAlign) + this.Config.DurationOffset;
                ImGuiEx.DrawText(drawList, duration, durationPos, this.Config.DurationColor.Base, this.Config.DurationShowOutline, this.Config.DurationOutlineColor.Base);
            }
        }

        var raidStatsSize = Vector2.Zero;
        if (this.Config.ShowRaidStats && encounter is not null)
        {
            var text = encounter.GetFormattedString($" {this.Config.RaidStatsFormat} ", this.Config.ThousandsSeparators ? "N" : "F");

            if (!string.IsNullOrEmpty(text))
            {
                using (FontsManager.PushFont(this.Config.StatsFontKey))
                {
                    raidStatsSize = ImGui.CalcTextSize(text);
                    var statsPos = Utils.GetAnchoredPosition(pos + this.Config.StatsOffset, -headerSize, DrawAnchor.Right);
                    statsPos = Utils.GetAnchoredPosition(statsPos, raidStatsSize, this.Config.StatsAlign);
                    ImGuiEx.DrawText(drawList, text, statsPos, this.Config.RaidStatsColor.Base, this.Config.StatsShowOutline, this.Config.StatsOutlineColor.Base);
                }
            }
        }

        if (this.Config.ShowEncounterName && encounter is not null && !string.IsNullOrEmpty(encounter.Title))
        {
            using (FontsManager.PushFont(this.Config.NameFontKey))
            {
                var name = $" {encounter.Name}";
                var nameSize = ImGui.CalcTextSize(name);

                if (durationSize.X + raidStatsSize.X + nameSize.X > size.X)
                {
                    var elipsesWidth = ImGui.CalcTextSize("... ").X;
                    do
                    {
                        name = name.AsSpan(0, name.Length - 1).ToString();
                        nameSize = ImGui.CalcTextSize(name);
                    }
                    while (durationSize.X + raidStatsSize.X + nameSize.X + elipsesWidth > size.X && name.Length > 1);

                    name += "... ";
                }

                var namePos = Utils.GetAnchoredPosition(pos.AddX(durationSize.X), -headerSize, DrawAnchor.Left);
                namePos = Utils.GetAnchoredPosition(namePos, nameSize, this.Config.NameAlign) + this.Config.NameOffset;
                ImGuiEx.DrawText(drawList, name, namePos, this.Config.NameColor.Base, this.Config.NameShowOutline, this.Config.NameOutlineColor.Base);
            }
        }

        return (pos.AddY(this.Config.HeaderHeight), size.AddY(-this.Config.HeaderHeight));
    }

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        var fontOptions = FontsManager.GetFontList();
        if (fontOptions.Length == 0)
        {
            return;
        }

        var change = false;
        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            if (ImGuiEx.Checkbox("Show Header", this.Config.ShowHeader, x => this.Config.ShowHeader = x))
            {
                change = true;
            }

            if (this.Config.ShowHeader)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.DragInt("Header Height", this.Config.HeaderHeight, x => this.Config.HeaderHeight = x, 1, 0, 100))
                {
                    change = true;
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.ColorEdit4("Background Color", this.Config.BackgroundColor.Vector, x => this.Config.BackgroundColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                {
                    change = true;
                }

                ImGui.NewLine();
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Checkbox("Show Encounter Duration", this.Config.ShowEncounterDuration, x => this.Config.ShowEncounterDuration = x))
                {
                    change = true;
                }

                if (this.Config.ShowEncounterDuration)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.DragFloat2("Position Offset##Duration", this.Config.DurationOffset, x => this.Config.DurationOffset = x))
                    {
                        change = true;
                    }

                    if (!FontsManager.ValidateFont(fontOptions, this.Config.DurationFontId, this.Config.DurationFontKey))
                    {
                        this.Config.DurationFontId = 0;
                        for (var i = 0; i < fontOptions.Length; i++)
                        {
                            if (this.Config.DurationFontKey.Equals(fontOptions[i]))
                            {
                                this.Config.DurationFontId = i;
                                change = true;
                            }
                        }
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Combo("Font##Duration", this.Config.DurationFontId, x => this.Config.DurationFontId = x, fontOptions))
                    {
                        this.Config.DurationFontKey = fontOptions[this.Config.DurationFontId];
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Combo("Text Align##Duration", this.Config.DurationAlign, x => this.Config.DurationAlign = x, AnchorOptions))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.ColorEdit4("Text Color##Duration", this.Config.DurationColor.Vector, x => this.Config.DurationColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Checkbox("Show Outline##Duration", this.Config.DurationShowOutline, x => this.Config.DurationShowOutline = x))
                    {
                        change = true;
                    }

                    if (this.Config.DurationShowOutline)
                    {
                        ImGuiEx.DrawNestIndicator(3);
                        if (ImGuiEx.ColorEdit4("Outline Color##Duration", this.Config.DurationOutlineColor.Vector, x => this.Config.DurationOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                        {
                            change = true;
                        }
                    }
                }

                ImGui.NewLine();
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Checkbox("Show Encounter Name", this.Config.ShowEncounterName, x => this.Config.ShowEncounterName = x))
                {
                    change = true;
                }

                if (this.Config.ShowEncounterName)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.DragFloat2("Position Offset##Name", this.Config.NameOffset, x => this.Config.NameOffset = x))
                    {
                        change = true;
                    }

                    if (!FontsManager.ValidateFont(fontOptions, this.Config.NameFontId, this.Config.NameFontKey))
                    {
                        this.Config.NameFontId = 0;
                        for (var i = 0; i < fontOptions.Length; i++)
                        {
                            if (this.Config.NameFontKey.Equals(fontOptions[i]))
                            {
                                this.Config.NameFontId = i;
                                change = true;
                            }
                        }
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Combo("Font##Name", this.Config.NameFontId, x => this.Config.NameFontId = x, fontOptions))
                    {
                        this.Config.NameFontKey = fontOptions[this.Config.NameFontId];
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Combo("Text Align##Name", this.Config.NameAlign, x => this.Config.NameAlign = x, AnchorOptions))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.ColorEdit4("Text Color##Name", this.Config.NameColor.Vector, x => this.Config.NameColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Checkbox("Show Outline##Name", this.Config.NameShowOutline, x => this.Config.NameShowOutline = x))
                    {
                        change = true;
                    }

                    if (this.Config.NameShowOutline)
                    {
                        ImGuiEx.DrawNestIndicator(3);
                        if (ImGuiEx.ColorEdit4("Outline Color##Name", this.Config.NameOutlineColor.Vector, x => this.Config.NameOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                        {
                            change = true;
                        }
                    }
                }

                ImGui.NewLine();
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Checkbox("Show Raid Stats", this.Config.ShowRaidStats, x => this.Config.ShowRaidStats = x))
                {
                    change = true;
                }

                if (this.Config.ShowRaidStats)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.InputText("Raid Stats Format", this.Config.RaidStatsFormat, x => this.Config.RaidStatsFormat = x, 128))
                    {
                        change = true;
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip(Utils.GetTagsTooltip(BaseEncounter.TextTags));
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Checkbox("Use Thousands Separators for Numbers", this.Config.ThousandsSeparators, x => this.Config.ThousandsSeparators = x))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.DragFloat2("Position Offset##Stats", this.Config.StatsOffset, x => this.Config.StatsOffset = x))
                    {
                        change = true;
                    }

                    if (!FontsManager.ValidateFont(fontOptions, this.Config.StatsFontId, this.Config.StatsFontKey))
                    {
                        this.Config.StatsFontId = 0;
                        for (var i = 0; i < fontOptions.Length; i++)
                        {
                            if (this.Config.StatsFontKey.Equals(fontOptions[i]))
                            {
                                this.Config.StatsFontId = i;
                                change = true;
                            }
                        }
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Combo("Font##Stats", this.Config.StatsFontId, x => this.Config.StatsFontId = x, fontOptions))
                    {
                        this.Config.StatsFontKey = fontOptions[this.Config.StatsFontId];
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Combo("Text Align##Stats", this.Config.StatsAlign, x => this.Config.StatsAlign = x, AnchorOptions))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.ColorEdit4("Text Color##Stats", this.Config.RaidStatsColor.Vector, x => this.Config.RaidStatsColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                    {
                        change = true;
                    }

                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.Checkbox("Show Outline##Stats", this.Config.StatsShowOutline, x => this.Config.StatsShowOutline = x))
                    {
                        change = true;
                    }

                    if (this.Config.StatsShowOutline)
                    {
                        ImGuiEx.DrawNestIndicator(3);
                        if (ImGuiEx.ColorEdit4("Outline Color##Stats", this.Config.StatsOutlineColor.Vector, x => this.Config.StatsOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                        {
                            change = true;
                        }
                    }
                }
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