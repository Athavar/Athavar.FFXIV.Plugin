// <copyright file="BarConfig.cs" company="Athavar">
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

internal class BarConfigPage : IConfigPage
{
    [JsonIgnore]
    private static readonly string[] AnchorOptions = Enum.GetNames(typeof(DrawAnchor));

    private static readonly string[] JobIconStyleOptions = Enum.GetNames(typeof(JobIconStyle)).Select(s => $"Style {s}").ToArray();

    private readonly MeterConfig config;
    private readonly IFontsManager fontsManager;
    private readonly IIconManager iconManager;

    public BarConfigPage(MeterConfig config, IFontsManager fontsManager, IIconManager iconManager)
    {
        this.config = config;
        this.fontsManager = fontsManager;
        this.iconManager = iconManager;
    }

    public string Name => "Bars";

    private BarConfig Config => this.config.BarConfig;

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
        Combatant combatant,
        ConfigColor jobColor,
        ConfigColor barColor,
        int barCount,
        float top,
        float current)
    {
        var barHeight = (size.Y - ((barCount - 1) * this.Config.BarGaps)) / barCount;
        var barSize = new Vector2(size.X, barHeight);
        var barFillSize = new Vector2(size.X * (current / top), barHeight);
        drawList.AddRectFilled(localPos, localPos + barFillSize, this.Config.UseJobColor ? jobColor.Base : barColor.Base);

        var textOffset = 5f;
        if (this.Config.ShowJobIcon && combatant.Job != Job.Adventurer)
        {
            var jobIconSize = Vector2.One * barHeight;
            this.DrawJobIcon(combatant.Job, (JobIconStyle)this.Config.JobIconStyle, localPos + this.Config.JobIconOffset, jobIconSize, drawList);
            textOffset = barHeight;
        }

        if (this.Config.ShowRankText)
        {
            var rankText = combatant.GetFormattedString($"{this.Config.RankTextFormat}", this.Config.ThousandsSeparators ? "N" : "F");
            using (FontsManager.PushFont(this.Config.RankTextFontKey))
            {
                textOffset += ImGui.CalcTextSize("00.").X;
                var rankTextSize = ImGui.CalcTextSize(rankText);
                var rankTextPos = Utils.GetAnchoredPosition(localPos, -barSize, DrawAnchor.Left);
                rankTextPos = Utils.GetAnchoredPosition(rankTextPos, rankTextSize, this.Config.RankTextAlign) + this.Config.RankTextOffset;
                ImGuiEx.DrawText(
                    drawList,
                    rankText,
                    rankTextPos.AddX(textOffset),
                    this.Config.RankTextJobColor ? jobColor.Base : this.Config.RankTextColor.Base,
                    this.Config.RankTextShowOutline,
                    this.Config.RankTextOutlineColor.Base);
            }
        }

        using (FontsManager.PushFont(this.Config.BarNameFontKey))
        {
            var leftText = combatant.GetFormattedString($" {this.Config.LeftTextFormat} ", this.Config.ThousandsSeparators ? "N" : "F");
            var nameTextSize = ImGui.CalcTextSize(leftText);
            var namePos = Utils.GetAnchoredPosition(localPos, -barSize, DrawAnchor.Left);
            namePos = Utils.GetAnchoredPosition(namePos, nameTextSize, DrawAnchor.Left) + this.Config.LeftTextOffset;
            ImGuiEx.DrawText(
                drawList,
                leftText,
                namePos.AddX(textOffset),
                this.Config.LeftTextJobColor ? jobColor.Base : this.Config.BarNameColor.Base,
                this.Config.BarNameShowOutline,
                this.Config.BarNameOutlineColor.Base);
        }

        using (FontsManager.PushFont(this.Config.BarDataFontKey))
        {
            var rightText = combatant.GetFormattedString($" {this.Config.RightTextFormat} ", this.Config.ThousandsSeparators ? "N" : "F");
            var dataTextSize = ImGui.CalcTextSize(rightText);
            var dataPos = Utils.GetAnchoredPosition(localPos, -barSize, DrawAnchor.Right);
            dataPos = Utils.GetAnchoredPosition(dataPos, dataTextSize, DrawAnchor.Right) + this.Config.RightTextOffset;
            ImGuiEx.DrawText(
                drawList,
                rightText,
                dataPos,
                this.Config.RightTextJobColor ? jobColor.Base : this.Config.BarDataColor.Base,
                this.Config.BarDataShowOutline,
                this.Config.BarDataOutlineColor.Base);
        }

        return localPos.AddY(barHeight + this.Config.BarGaps);
    }

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        var fontOptions = FontsManager.GetFontList();
        if (fontOptions.Length == 0)
        {
            return;
        }

        if (ImGui.BeginChild($"##{this.Name}", new Vector2(size.X, size.Y), true))
        {
            ImGuiEx.DragInt("Num Bars to Display", this.Config.BarCount, x => this.Config.BarCount = x, 1, 1, 48);
            ImGuiEx.DragInt("Bar Gap Size", this.Config.BarGaps, x => this.Config.BarGaps = x, 1, 0, 20);

            ImGuiEx.Checkbox("Show Job Icon", this.Config.ShowJobIcon, x => this.Config.ShowJobIcon = x);
            if (this.Config.ShowJobIcon)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.DragFloat2("Job Icon Offset", this.Config.JobIconOffset, x => this.Config.JobIconOffset = x);

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.Combo("Job Icon Style", this.Config.JobIconStyle, x => this.Config.JobIconStyle = x, JobIconStyleOptions);
            }

            ImGuiEx.Checkbox("Use Job Colors for Bars", this.Config.UseJobColor, x => this.Config.UseJobColor = x);
            if (!this.Config.UseJobColor)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Bar Color", this.Config.BarColor.Vector, x => this.Config.BarColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
            }

            ImGuiEx.Checkbox("Use Thousands Separators for Numbers", this.Config.ThousandsSeparators, x => this.Config.ThousandsSeparators = x);

            ImGui.NewLine();
            ImGuiEx.Checkbox("Show Rank Text", this.Config.ShowRankText, x => this.Config.ShowRankText = x);
            if (this.Config.ShowRankText)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.InputText("Rank Text Format", this.Config.RankTextFormat, x => this.Config.RankTextFormat = x, 128);

                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(Utils.GetTagsTooltip(Combatant.TextTags));
                }

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.Combo("Rank Text Align", this.Config.RankTextAlign, x => this.Config.RankTextAlign = x, AnchorOptions);

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.DragFloat2("Rank Text Offset", this.Config.RankTextOffset, x => this.Config.RankTextOffset = x);

                if (!FontsManager.ValidateFont(fontOptions, this.Config.RankTextFontId, this.Config.RankTextFontKey))
                {
                    this.Config.RankTextFontId = 0;
                    for (var i = 0; i < fontOptions.Length; i++)
                    {
                        if (this.Config.RankTextFontKey.Equals(fontOptions[i]))
                        {
                            this.Config.RankTextFontId = i;
                        }
                    }
                }

                ImGuiEx.DrawNestIndicator(1);
                if (ImGuiEx.Combo("Font##Rank", this.Config.RankTextFontId, x => this.Config.RankTextFontId = x, fontOptions))
                {
                    this.Config.RankTextFontKey = fontOptions[this.Config.RankTextFontId];
                }

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.Checkbox("Use Job Color##RankTextJobColor", this.Config.RankTextJobColor, x => this.Config.RankTextJobColor = x);
                if (!this.Config.RankTextJobColor)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    ImGuiEx.ColorEdit4("Text Color##Rank", this.Config.RankTextColor.Vector, x => this.Config.RankTextColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                }

                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.Checkbox("Show Outline##Rank", this.Config.RankTextShowOutline, x => this.Config.RankTextShowOutline = x);
                if (this.Config.RankTextShowOutline)
                {
                    ImGuiEx.DrawNestIndicator(2);
                    ImGuiEx.ColorEdit4("Outline Color##Rank", this.Config.RankTextOutlineColor.Vector, x => this.Config.RankTextOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
                }
            }

            ImGui.NewLine();
            ImGuiEx.InputText("Left Text Format", this.Config.LeftTextFormat, x => this.Config.LeftTextFormat = x, 128);

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Utils.GetTagsTooltip(Combatant.TextTags));
            }

            ImGuiEx.DragFloat2("Left Text Offset", this.Config.LeftTextOffset, x => this.Config.LeftTextOffset = x);

            if (!FontsManager.ValidateFont(fontOptions, this.Config.BarNameFontId, this.Config.BarNameFontKey))
            {
                this.Config.BarNameFontId = 0;
                for (var i = 0; i < fontOptions.Length; i++)
                {
                    if (this.Config.BarNameFontKey.Equals(fontOptions[i]))
                    {
                        this.Config.BarNameFontId = i;
                    }
                }
            }

            ImGuiEx.Combo("Font##Name", this.Config.BarNameFontId, x => this.Config.BarNameFontId = x, fontOptions);
            this.Config.BarNameFontKey = fontOptions[this.Config.BarNameFontId];

            ImGuiEx.Checkbox("Use Job Color##LeftTextJobColor", this.Config.LeftTextJobColor, x => this.Config.LeftTextJobColor = x);
            if (!this.Config.LeftTextJobColor)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Text Color##Name", this.Config.BarNameColor.Vector, x => this.Config.BarNameColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
            }

            ImGuiEx.Checkbox("Show Outline##Name", this.Config.BarNameShowOutline, x => this.Config.BarNameShowOutline = x);
            if (this.Config.BarNameShowOutline)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Outline Color##Name", this.Config.BarNameOutlineColor.Vector, x => this.Config.BarNameOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
            }

            ImGui.NewLine();
            ImGuiEx.InputText("Right Text Format", this.Config.RightTextFormat, x => this.Config.RightTextFormat = x, 128);

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip(Utils.GetTagsTooltip(Combatant.TextTags));
            }

            ImGuiEx.DragFloat2("Right Text Offset", this.Config.RightTextOffset, x => this.Config.RightTextOffset = x);

            if (!FontsManager.ValidateFont(fontOptions, this.Config.BarDataFontId, this.Config.BarDataFontKey))
            {
                this.Config.BarDataFontId = 0;
                for (var i = 0; i < fontOptions.Length; i++)
                {
                    if (this.Config.BarDataFontKey.Equals(fontOptions[i]))
                    {
                        this.Config.BarDataFontId = i;
                    }
                }
            }

            if (ImGuiEx.Combo("Font##Data", this.Config.BarDataFontId, x => this.Config.BarDataFontId = x, fontOptions))
            {
                this.Config.BarDataFontKey = fontOptions[this.Config.BarDataFontId];
            }

            ImGuiEx.Checkbox("Use Job Color##RightTextJobColor", this.Config.RightTextJobColor, x => this.Config.RightTextJobColor = x);
            if (!this.Config.RightTextJobColor)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Text Color##Data", this.Config.BarDataColor.Vector, x => this.Config.BarDataColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
            }

            ImGuiEx.Checkbox("Show Outline##Data", this.Config.BarDataShowOutline, x => this.Config.BarDataShowOutline = x);
            if (this.Config.BarDataShowOutline)
            {
                ImGuiEx.DrawNestIndicator(1);
                ImGuiEx.ColorEdit4("Outline Color##Data", this.Config.BarDataOutlineColor.Vector, x => this.Config.BarDataOutlineColor.Vector = x, ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.AlphaBar);
            }
        }

        ImGui.EndChild();
    }

    private void DrawJobIcon(Job job, JobIconStyle style, Vector2 position, Vector2 size, ImDrawListPtr drawList)
    {
        if (!this.iconManager.TryGetJobIcon(job, style, true, out var tex))
        {
            return;
        }

        drawList.AddImage(tex.ImGuiHandle, position, position + size, Vector2.Zero, Vector2.One);
    }
}