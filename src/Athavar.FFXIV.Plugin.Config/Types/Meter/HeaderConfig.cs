// <copyright file="HeaderConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;
using System.Text.Json.Serialization;

public sealed class HeaderConfig : BaseConfig
{
    [JsonInclude]
    [JsonPropertyName("ShowHeader")]
    public bool ShowHeader { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("HeaderHeight")]
    public int HeaderHeight { get; set; } = 25;

    [JsonInclude]
    [JsonPropertyName("BackgroundColor")]
    public ConfigColor BackgroundColor { get; set; } = new(30f / 255f, 30f / 255f, 30f / 255f, 230 / 255f);

    [JsonInclude]
    [JsonPropertyName("ShowEncounterDuration")]
    public bool ShowEncounterDuration { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("DurationColor")]
    public ConfigColor DurationColor { get; set; } = new(0f / 255f, 190f / 255f, 225f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("DurationShowOutline")]
    public bool DurationShowOutline { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("DurationOutlineColor")]
    public ConfigColor DurationOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("DurationAlign")]
    public DrawAnchor DurationAlign { get; set; } = DrawAnchor.Left;

    [JsonInclude]
    [JsonPropertyName("DurationOffset")]
    public Vector2 DurationOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("DurationFontId")]
    public int DurationFontId { get; set; }

    [JsonInclude]
    [JsonPropertyName("DurationFontKey")]
    public string DurationFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    [JsonInclude]
    [JsonPropertyName("ShowEncounterName")]
    public bool ShowEncounterName { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("NameColor")]
    public ConfigColor NameColor { get; set; } = new(1, 1, 1, 1);

    [JsonInclude]
    [JsonPropertyName("NameShowOutline")]
    public bool NameShowOutline { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("NameOutlineColor")]
    public ConfigColor NameOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("NameAlign")]
    public DrawAnchor NameAlign { get; set; } = DrawAnchor.Left;

    [JsonInclude]
    [JsonPropertyName("NameOffset")]
    public Vector2 NameOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("NameFontId")]
    public int NameFontId { get; set; }

    [JsonInclude]
    [JsonPropertyName("NameFontKey")]
    public string NameFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    [JsonInclude]
    [JsonPropertyName("ShowRaidStats")]
    public bool ShowRaidStats { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("RaidStatsColor")]
    public ConfigColor RaidStatsColor { get; set; } = new(0.5f, 0.5f, 0.5f, 1f);

    [JsonInclude]
    [JsonPropertyName("StatsShowOutline")]
    public bool StatsShowOutline { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("StatsOutlineColor")]
    public ConfigColor StatsOutlineColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("StatsAlign")]
    public DrawAnchor StatsAlign { get; set; } = DrawAnchor.Right;

    [JsonInclude]
    [JsonPropertyName("StatsOffset")]
    public Vector2 StatsOffset { get; set; } = new(0, 0);

    [JsonInclude]
    [JsonPropertyName("StatsFontId")]
    public int StatsFontId { get; set; }

    [JsonInclude]
    [JsonPropertyName("StatsFontKey")]
    public string StatsFontKey { get; set; } = Constants.FontsManager.DalamudFontKey;

    [JsonInclude]
    [JsonPropertyName("RaidStatsFormat")]
    public string RaidStatsFormat { get; set; } = "[dps]rdps [hps]rhps Deaths: [deaths]";

    [JsonInclude]
    [JsonPropertyName("ThousandsSeparators")]
    public bool ThousandsSeparators { get; set; } = true;
}