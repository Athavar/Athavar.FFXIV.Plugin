// <copyright file="HeaderConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;

public class HeaderConfig : BaseConfig
{
    public bool ShowHeader { get; set; } = true;

    public int HeaderHeight { get; set; } = 25;

    public ConfigColor BackgroundColor { get; set; } = new(30f / 255f, 30f / 255f, 30f / 255f, 230 / 255f);

    public bool ShowEncounterDuration { get; set; } = true;

    public ConfigColor DurationColor { get; set; } = new(0f / 255f, 190f / 255f, 225f / 255f, 1f);

    public bool DurationShowOutline { get; set; } = true;

    public ConfigColor DurationOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    public DrawAnchor DurationAlign { get; set; } = DrawAnchor.Left;

    public Vector2 DurationOffset { get; set; } = new(0, 0);

    public int DurationFontId { get; set; } = 0;

    public string DurationFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    public bool ShowEncounterName { get; set; } = true;

    public ConfigColor NameColor { get; set; } = new(1, 1, 1, 1);

    public bool NameShowOutline { get; set; } = true;

    public ConfigColor NameOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    public DrawAnchor NameAlign { get; set; } = DrawAnchor.Left;

    public Vector2 NameOffset { get; set; } = new(0, 0);

    public int NameFontId { get; set; } = 0;

    public string NameFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    public bool ShowRaidStats { get; set; } = true;

    public ConfigColor RaidStatsColor { get; set; } = new(0.5f, 0.5f, 0.5f, 1f);

    public bool StatsShowOutline { get; set; } = true;

    public ConfigColor StatsOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    public DrawAnchor StatsAlign { get; set; } = DrawAnchor.Right;

    public Vector2 StatsOffset { get; set; } = new(0, 0);

    public int StatsFontId { get; set; } = 0;

    public string StatsFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    public string RaidStatsFormat { get; set; } = "[dps]rdps [hps]rhps Deaths: [deaths]";

    public bool ThousandsSeparators { get; set; } = true;
}