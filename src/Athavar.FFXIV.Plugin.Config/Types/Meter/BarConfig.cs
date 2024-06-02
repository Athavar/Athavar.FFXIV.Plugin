// <copyright file="BarConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;
using System.Text.Json.Serialization;

public sealed class BarConfig : BaseConfig
{
    [JsonInclude]
    [JsonPropertyName("BarCount")]
    public int BarCount { get; set; } = 9;

    [JsonInclude]
    [JsonPropertyName("MinBarCount")]
    public int MinBarCount { get; set; } = 4;

    [JsonInclude]
    [JsonPropertyName("BarGaps")]
    public int BarGaps { get; set; } = 1;

    [JsonInclude]
    [JsonPropertyName("ShowJobIcon")]
    public bool ShowJobIcon { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("JobIconStyle")]
    public int JobIconStyle { get; set; }

    [JsonInclude]
    [JsonPropertyName("JobIconOffset")]
    public Vector2 JobIconOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("ThousandsSeparators")]
    public bool ThousandsSeparators { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("UseJobColor")]
    public bool UseJobColor { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("BarColor")]
    public ConfigColor BarColor { get; set; } = new(.3f, .3f, .3f, 1f);

    [JsonInclude]
    [JsonPropertyName("ShowRankText")]
    public bool ShowRankText { get; set; }

    [JsonInclude]
    [JsonPropertyName("RankTextFormat")]
    public string RankTextFormat { get; set; } = "[rank].";

    [JsonInclude]
    [JsonPropertyName("RankTextAlign")]
    public DrawAnchor RankTextAlign { get; set; } = DrawAnchor.Right;

    [JsonInclude]
    [JsonPropertyName("RankTextOffset")]
    public Vector2 RankTextOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("RankTextJobColor")]
    public bool RankTextJobColor { get; set; }

    [JsonInclude]
    [JsonPropertyName("RankTextColor")]
    public ConfigColor RankTextColor { get; set; } = new(1, 1, 1, 1);

    [JsonInclude]
    [JsonPropertyName("RankTextShowOutline")]
    public bool RankTextShowOutline { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("RankTextOutlineColor")]
    public ConfigColor RankTextOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("RankTextFontKey")]
    public string RankTextFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    [JsonInclude]
    [JsonPropertyName("RankTextFontId")]
    public int RankTextFontId { get; set; }

    [JsonInclude]
    [JsonPropertyName("LeftTextFormat")]
    public string LeftTextFormat { get; set; } = "[name]";

    [JsonInclude]
    [JsonPropertyName("LeftTextOffset")]
    public Vector2 LeftTextOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("LeftTextJobColor")]
    public bool LeftTextJobColor { get; set; }

    [JsonInclude]
    [JsonPropertyName("BarNameColor")]
    public ConfigColor BarNameColor { get; set; } = new(1, 1, 1, 1);

    [JsonInclude]
    [JsonPropertyName("BarNameShowOutline")]
    public bool BarNameShowOutline { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("BarNameOutlineColor")]
    public ConfigColor BarNameOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("BarNameFontKey")]
    public string BarNameFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    [JsonInclude]
    [JsonPropertyName("BarNameFontId")]
    public int BarNameFontId { get; set; }

    [JsonInclude]
    [JsonPropertyName("RightTextFormat")]
    public string RightTextFormat { get; set; } = "[damagetotal]  ([dps])";

    [JsonInclude]
    [JsonPropertyName("RightTextOffset")]
    public Vector2 RightTextOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("RightTextJobColor")]
    public bool RightTextJobColor { get; set; }

    [JsonInclude]
    [JsonPropertyName("BarDataColor")]
    public ConfigColor BarDataColor { get; set; } = new(1, 1, 1, 1);

    [JsonInclude]
    [JsonPropertyName("BarDataShowOutline")]
    public bool BarDataShowOutline { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("BarDataOutlineColor")]
    public ConfigColor BarDataOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("BarDataFontKey")]
    public string BarDataFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    [JsonInclude]
    [JsonPropertyName("BarDataFontId")]
    public int BarDataFontId { get; set; }
}