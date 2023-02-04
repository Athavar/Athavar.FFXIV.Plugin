// <copyright file="BarConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;

public class BarConfig : BaseConfig
{
    public int BarCount { get; set; } = 10;

    public int BarGaps { get; set; } = 1;

    public bool ShowJobIcon { get; set; } = true;

    public int JobIconStyle { get; set; }

    public Vector2 JobIconOffset { get; set; } = new(0, 0);

    public bool ThousandsSeparators { get; set; } = true;

    public bool UseJobColor { get; set; } = true;

    public ConfigColor BarColor { get; set; } = new(.3f, .3f, .3f, 1f);

    public bool ShowRankText { get; set; }

    public string RankTextFormat { get; set; } = "[rank].";

    public DrawAnchor RankTextAlign { get; set; } = DrawAnchor.Right;

    public Vector2 RankTextOffset { get; set; } = new(0, 0);

    public bool RankTextJobColor { get; set; }

    public ConfigColor RankTextColor { get; set; } = new(1, 1, 1, 1);

    public bool RankTextShowOutline { get; set; } = true;

    public ConfigColor RankTextOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    public string RankTextFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    public int RankTextFontId { get; set; }

    public string LeftTextFormat { get; set; } = "[name]";

    public Vector2 LeftTextOffset { get; set; } = new(0, 0);

    public bool LeftTextJobColor { get; set; }

    public ConfigColor BarNameColor { get; set; } = new(1, 1, 1, 1);

    public bool BarNameShowOutline { get; set; } = true;

    public ConfigColor BarNameOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    public string BarNameFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    public int BarNameFontId { get; set; }

    public string RightTextFormat { get; set; } = "[damagetotal]  ([dps])";

    public Vector2 RightTextOffset { get; set; } = new(0, 0);

    public bool RightTextJobColor { get; set; }

    public ConfigColor BarDataColor { get; set; } = new(1, 1, 1, 1);

    public bool BarDataShowOutline { get; set; } = true;

    public ConfigColor BarDataOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    public string BarDataFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    public int BarDataFontId { get; set; }
}