// <copyright file="BarConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game.ClientState.Keys;
using ImGuiNET;

internal sealed class BarConfigPage : IConfigPage
{
    [JsonIgnore]
    private static readonly string[] AnchorOptions = Enum.GetNames(typeof(DrawAnchor));

    private static readonly string[] JobIconStyleOptions = Enum.GetNames(typeof(JobIconStyle)).Select(s => $"Style {s}").ToArray();

    private readonly MeterWindow window;
    private readonly IDalamudServices services;
    private readonly Utils utils;
    private readonly IFontsManager fontsManager;
    private readonly IIconManager iconManager;

    private readonly Dictionary<string, Cache> cache = new();

    private float? rankTestSizeOffset;

    public BarConfigPage(MeterWindow window, IDalamudServices services, Utils utils, IFontsManager fontsManager, IIconManager iconManager)
    {
        this.window = window;
        this.services = services;
        this.fontsManager = fontsManager;
        this.iconManager = iconManager;
        this.utils = utils;
    }

    public string Name => "Bars";

    private BarConfig Config => this.window.Config.BarConfig;

    public IConfig GetDefault()
    {
        var c = new BarConfig
        {
            BarNameFontKey = FontsManager.DefaultSmallFontKey,
            BarNameFontId = this.fontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
            BarDataFontKey = FontsManager.DefaultSmallFontKey,
            BarDataFontId = this.fontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
            RankTextFontKey = FontsManager.DefaultSmallFontKey,
            RankTextFontId = this.fontsManager.GetFontIndex(FontsManager.DefaultSmallFontKey),
        };

        return c;
    }

    public IConfig GetConfig() => this.Config;

    public Vector2 DrawBar(
        ImDrawListPtr drawList,
        Vector2 localPos,
        Vector2 size,
        BaseCombatant baseCombatant,
        ConfigColor jobColor,
        ConfigColor barColor,
        int barCount,
        float top,
        float current)
    {
        var barHeight = (size.Y - ((barCount - 1) * this.Config.BarGaps)) / barCount;
        var barSize = size with { Y = barHeight };
        var barFillSize = new Vector2(size.X * (current / top), barHeight);
        drawList.AddRectFilled(localPos, localPos + barFillSize, this.Config.UseJobColor ? jobColor.Base : barColor.Base);

        var general = this.window.Config.GeneralConfig;
        var keyState = this.services.KeyState;
        if (general.ShowActionSummaryTooltip && (general.ShowActionSummaryModifyKey == (ushort)VirtualKey.NO_KEY || keyState[general.ShowActionSummaryModifyKey]) && ImGui.IsMouseHoveringRect(localPos, localPos + barFillSize))
        {
            this.utils.DrawActionSummaryTooltip(baseCombatant, true, general.DataType);
        }

        if (!this.cache.TryGetValue(baseCombatant.Name, out var cache))
        {
            cache = new Cache();
            this.cache.Add(baseCombatant.Name, cache);
        }

        var textOffset = 5f;
        if (this.Config.ShowJobIcon && baseCombatant.Job != Job.Adventurer)
        {
            var iconSize = Math.Min(barHeight, size.X / 4);
            var iconTopOffset = (barHeight - iconSize) / 2;
            var iconPos = localPos with { Y = localPos.Y + iconTopOffset };
            var jobIconSize = Vector2.One * iconSize;
            this.DrawJobIcon(baseCombatant.Job, (JobIconStyle)this.Config.JobIconStyle, iconPos + this.Config.JobIconOffset, jobIconSize, drawList);
            textOffset = iconSize;
        }

        if (this.Config.ShowRankText)
        {
            cache.Rank ??= baseCombatant.GetFormattedString($"{this.Config.RankTextFormat}", this.Config.ThousandsSeparators ? "N" : "F");
            using (FontsManager.PushFont(this.Config.RankTextFontKey))
            {
                textOffset += ImGui.CalcTextSize("00.").X;
                var rankTextSize = ImGui.CalcTextSize(cache.Rank);
                var rankTextPos = Utils.GetAnchoredPosition(localPos, -barSize, DrawAnchor.Left);
                rankTextPos = Utils.GetAnchoredPosition(rankTextPos, rankTextSize, this.Config.RankTextAlign) + this.Config.RankTextOffset;
                ImGuiEx.DrawText(
                    drawList,
                    cache.Rank,
                    rankTextPos.AddX(textOffset),
                    this.Config.RankTextJobColor ? jobColor.Base : this.Config.RankTextColor.Base,
                    this.Config.RankTextShowOutline,
                    this.Config.RankTextOutlineColor.Base);
            }
        }

        using (FontsManager.PushFont(this.Config.BarNameFontKey))
        {
            cache.Left ??= baseCombatant.GetFormattedString($" {this.Config.LeftTextFormat} ", this.Config.ThousandsSeparators ? "N" : "F");
            var nameTextSize = ImGui.CalcTextSize(cache.Left);
            var namePos = Utils.GetAnchoredPosition(localPos, -barSize, DrawAnchor.Left);
            namePos = Utils.GetAnchoredPosition(namePos, nameTextSize, DrawAnchor.Left) + this.Config.LeftTextOffset;
            ImGuiEx.DrawText(
                drawList,
                cache.Left,
                namePos.AddX(textOffset),
                this.Config.LeftTextJobColor ? jobColor.Base : this.Config.BarNameColor.Base,
                this.Config.BarNameShowOutline,
                this.Config.BarNameOutlineColor.Base);
        }

        using (FontsManager.PushFont(this.Config.BarDataFontKey))
        {
            cache.Right ??= baseCombatant.GetFormattedString($" {this.Config.RightTextFormat} ", this.Config.ThousandsSeparators ? "N" : "F");
            var dataTextSize = ImGui.CalcTextSize(cache.Right);
            var dataPos = Utils.GetAnchoredPosition(localPos, -barSize, DrawAnchor.Right);
            dataPos = Utils.GetAnchoredPosition(dataPos, dataTextSize, DrawAnchor.Right) + this.Config.RightTextOffset;
            ImGuiEx.DrawText(
                drawList,
                cache.Right,
                dataPos,
                this.Config.RightTextJobColor ? jobColor.Base : this.Config.BarDataColor.Base,
                this.Config.BarDataShowOutline,
                this.Config.BarDataOutlineColor.Base);
        }

        return localPos.AddY(barHeight + this.Config.BarGaps);
    }

    public void CacheReset() => this.cache.Clear();

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        var fontOptions = FontsManager.GetFontList();
        if (fontOptions.Length == 0)
        {
            return;
        }

        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            var change = false;
            if (ImGuiEx.DragIntRange2("Limit Num Bars to Display", this.Config.MinBarCount, x => this.Config.MinBarCount = x, this.Config.BarCount, x => this.Config.BarCount = x, 1, 1, 48))
            {
                change = true;
            }

            if (ImGuiEx.DragInt("Bar Gap Size", this.Config.BarGaps, x => this.Config.BarGaps = x, 1, 0, 20))
            {
                change = true;
            }

            if (ImGuiEx.Checkbox("Show Job Icon", this.Config.ShowJobIcon, x => this.Config.ShowJobIcon = x))
            {
                change = true;
            }

            if (this.Config.ShowJobIcon)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.DragFloat2("Job Icon Offset", this.Config.JobIconOffset, x => this.Config.JobIconOffset = x))
                {
                    change = true;
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Combo("Job Icon Style", this.Config.JobIconStyle, x => this.Config.JobIconStyle = x, JobIconStyleOptions))
                {
                    change = true;
                }
            }

            if (ImGuiEx.Checkbox("Use Job Colors for Bars", this.Config.UseJobColor, x => this.Config.UseJobColor = x))
            {
                change = true;
            }

            if (!this.Config.UseJobColor)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.ColorEdit4("Bar Color", this.Config.BarColor.Vector, x => this.Config.BarColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                {
                    change = true;
                }
            }

            if (ImGuiEx.Checkbox("Use Thousands Separators for Numbers", this.Config.ThousandsSeparators, x => this.Config.ThousandsSeparators = x))
            {
                change = true;
            }

            ImGui.NewLine();
            if (ImGuiEx.Checkbox("Show Rank Text", this.Config.ShowRankText, x => this.Config.ShowRankText = x))
            {
                change = true;
            }

            if (this.Config.ShowRankText)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.InputText("Rank Text Format", this.Config.RankTextFormat, x => this.Config.RankTextFormat = x, 128))
                {
                    change = true;
                }

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Utils.GetTagsTooltip(BaseCombatant.TextTags));
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Combo("Rank Text Align", this.Config.RankTextAlign, x => this.Config.RankTextAlign = x, AnchorOptions))
                {
                    change = true;
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.DragFloat2("Rank Text Offset", this.Config.RankTextOffset, x => this.Config.RankTextOffset = x))
                {
                    change = true;
                }

                if (!FontsManager.ValidateFont(fontOptions, this.Config.RankTextFontId, this.Config.RankTextFontKey))
                {
                    this.Config.RankTextFontId = 0;
                    for (var i = 0; i < fontOptions.Length; i++)
                    {
                        if (this.Config.RankTextFontKey.Equals(fontOptions[i]))
                        {
                            this.Config.RankTextFontId = i;
                            change = true;
                        }
                    }
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Combo("Font##Rank", this.Config.RankTextFontId, x => this.Config.RankTextFontId = x, fontOptions))
                {
                    this.Config.RankTextFontKey = fontOptions[this.Config.RankTextFontId];
                    change = true;
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Checkbox("Use Job Color##RankTextJobColor", this.Config.RankTextJobColor, x => this.Config.RankTextJobColor = x))
                {
                    change = true;
                }

                if (!this.Config.RankTextJobColor)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.ColorEdit4("Text Color##Rank", this.Config.RankTextColor.Vector, x => this.Config.RankTextColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                    {
                        change = true;
                    }
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Checkbox("Show Outline##Rank", this.Config.RankTextShowOutline, x => this.Config.RankTextShowOutline = x))
                {
                    change = true;
                }

                if (this.Config.RankTextShowOutline)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    if (ImGuiEx.ColorEdit4("Outline Color##Rank", this.Config.RankTextOutlineColor.Vector, x => this.Config.RankTextOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                    {
                        change = true;
                    }
                }
            }

            ImGui.NewLine();
            if (ImGuiEx.InputText("Left Text Format", this.Config.LeftTextFormat, x => this.Config.LeftTextFormat = x, 128))
            {
                change = true;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Utils.GetTagsTooltip(BaseCombatant.TextTags));
            }

            if (ImGuiEx.DragFloat2("Left Text Offset", this.Config.LeftTextOffset, x => this.Config.LeftTextOffset = x))
            {
                change = true;
            }

            if (!FontsManager.ValidateFont(fontOptions, this.Config.BarNameFontId, this.Config.BarNameFontKey))
            {
                this.Config.BarNameFontId = 0;
                for (var i = 0; i < fontOptions.Length; i++)
                {
                    if (this.Config.BarNameFontKey.Equals(fontOptions[i]))
                    {
                        this.Config.BarNameFontId = i;
                        change = true;
                    }
                }
            }

            if (ImGuiEx.Combo("Font##Name", this.Config.BarNameFontId, x => this.Config.BarNameFontId = x, fontOptions))
            {
                this.Config.BarNameFontKey = fontOptions[this.Config.BarNameFontId];
                change = true;
            }

            if (ImGuiEx.Checkbox("Use Job Color##LeftTextJobColor", this.Config.LeftTextJobColor, x => this.Config.LeftTextJobColor = x))
            {
                change = true;
            }

            if (!this.Config.LeftTextJobColor)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.ColorEdit4("Text Color##Name", this.Config.BarNameColor.Vector, x => this.Config.BarNameColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                {
                    change = true;
                }
            }

            if (ImGuiEx.Checkbox("Show Outline##Name", this.Config.BarNameShowOutline, x => this.Config.BarNameShowOutline = x))
            {
                change = true;
            }

            if (this.Config.BarNameShowOutline)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Outline Color##Name", this.Config.BarNameOutlineColor.Vector, x => this.Config.BarNameOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
            }

            ImGui.NewLine();
            if (ImGuiEx.InputText("Right Text Format", this.Config.RightTextFormat, x => this.Config.RightTextFormat = x, 128))
            {
                change = true;
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Utils.GetTagsTooltip(BaseCombatant.TextTags));
            }

            if (ImGuiEx.DragFloat2("Right Text Offset", this.Config.RightTextOffset, x => this.Config.RightTextOffset = x))
            {
                change = true;
            }

            if (!FontsManager.ValidateFont(fontOptions, this.Config.BarDataFontId, this.Config.BarDataFontKey))
            {
                this.Config.BarDataFontId = 0;
                for (var i = 0; i < fontOptions.Length; i++)
                {
                    if (this.Config.BarDataFontKey.Equals(fontOptions[i]))
                    {
                        this.Config.BarDataFontId = i;
                        change = true;
                    }
                }
            }

            if (ImGuiEx.Combo("Font##Data", this.Config.BarDataFontId, x => this.Config.BarDataFontId = x, fontOptions))
            {
                this.Config.BarDataFontKey = fontOptions[this.Config.BarDataFontId];
                change = true;
            }

            if (ImGuiEx.Checkbox("Use Job Color##RightTextJobColor", this.Config.RightTextJobColor, x => this.Config.RightTextJobColor = x))
            {
                change = true;
            }

            if (!this.Config.RightTextJobColor)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.ColorEdit4("Text Color##Data", this.Config.BarDataColor.Vector, x => this.Config.BarDataColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                {
                    change = true;
                }
            }

            if (ImGuiEx.Checkbox("Show Outline##Data", this.Config.BarDataShowOutline, x => this.Config.BarDataShowOutline = x))
            {
                change = true;
            }

            if (this.Config.BarDataShowOutline)
            {
                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.ColorEdit4("Outline Color##Data", this.Config.BarDataOutlineColor.Vector, x => this.Config.BarDataOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar))
                {
                    change = true;
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

    private void DrawJobIcon(Job job, JobIconStyle style, Vector2 position, Vector2 size, ImDrawListPtr drawList)
    {
        if (!this.iconManager.TryGetJobIcon(job, style, true, out var tex) || !tex.TryGetWrap(out var textureWrap, out _))
        {
            return;
        }

        drawList.AddImage(textureWrap.ImGuiHandle, position, position + size, Vector2.Zero, Vector2.One);
    }

    private class Cache
    {
        public string? Rank { get; set; }

        public string? Left { get; set; }

        public string? Right { get; set; }
    }
}