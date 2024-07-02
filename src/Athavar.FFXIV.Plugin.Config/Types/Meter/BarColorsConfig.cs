// <copyright file="BarColorsConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable InconsistentNaming
namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

public sealed class BarColorsConfig : BaseConfig
{
    [JsonInclude]
    [JsonPropertyName("PLDColor")]
    public ConfigColor PLDColor { get; set; } = new(168f / 255f, 210f / 255f, 230f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("DRKColor")]
    public ConfigColor DRKColor { get; set; } = new(209f / 255f, 38f / 255f, 204f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("WARColor")]
    public ConfigColor WARColor { get; set; } = new(207f / 255f, 38f / 255f, 33f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("GNBColor")]
    public ConfigColor GNBColor { get; set; } = new(121f / 255f, 109f / 255f, 48f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("GLAColor")]
    public ConfigColor GLAColor { get; set; } = new(168f / 255f, 210f / 255f, 230f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("MRDColor")]
    public ConfigColor MRDColor { get; set; } = new(207f / 255f, 38f / 255f, 33f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("SCHColor")]
    public ConfigColor SCHColor { get; set; } = new(134f / 255f, 87f / 255f, 255f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("WHMColor")]
    public ConfigColor WHMColor { get; set; } = new(255f / 255f, 240f / 255f, 220f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("ASTColor")]
    public ConfigColor ASTColor { get; set; } = new(255f / 255f, 231f / 255f, 74f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("SGEColor")]
    public ConfigColor SGEColor { get; set; } = new(144f / 255f, 176f / 255f, 255f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("CNJColor")]
    public ConfigColor CNJColor { get; set; } = new(255f / 255f, 240f / 255f, 220f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("MNKColor")]
    public ConfigColor MNKColor { get; set; } = new(214f / 255f, 156f / 255f, 0f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("NINColor")]
    public ConfigColor NINColor { get; set; } = new(175f / 255f, 25f / 255f, 100f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("DRGColor")]
    public ConfigColor DRGColor { get; set; } = new(65f / 255f, 100f / 255f, 205f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("SAMColor")]
    public ConfigColor SAMColor { get; set; } = new(228f / 255f, 109f / 255f, 4f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("RPRColor")]
    public ConfigColor RPRColor { get; set; } = new(150f / 255f, 90f / 255f, 144f / 255f, 1f);

    // TODO: set custom color for viper
    [JsonInclude]
    [JsonPropertyName("VPRColor")]
    public ConfigColor VPRColor { get; set; } = new(150f / 255f, 90f / 255f, 144f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("PGLColor")]
    public ConfigColor PGLColor { get; set; } = new(214f / 255f, 156f / 255f, 0f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("ROGColor")]
    public ConfigColor ROGColor { get; set; } = new(175f / 255f, 25f / 255f, 100f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("LNCColor")]
    public ConfigColor LNCColor { get; set; } = new(65f / 255f, 100f / 255f, 205f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("BRDColor")]
    public ConfigColor BRDColor { get; set; } = new(145f / 255f, 186f / 255f, 94f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("MCHColor")]
    public ConfigColor MCHColor { get; set; } = new(110f / 255f, 225f / 255f, 214f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("DNCColor")]
    public ConfigColor DNCColor { get; set; } = new(226f / 255f, 176f / 255f, 175f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("ARCColor")]
    public ConfigColor ARCColor { get; set; } = new(145f / 255f, 186f / 255f, 94f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("BLMColor")]
    public ConfigColor BLMColor { get; set; } = new(165f / 255f, 121f / 255f, 214f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("SMNColor")]
    public ConfigColor SMNColor { get; set; } = new(45f / 255f, 155f / 255f, 120f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("RDMColor")]
    public ConfigColor RDMColor { get; set; } = new(232f / 255f, 123f / 255f, 123f / 255f, 1f);

    // TODO: set custom color for pictomancer
    [JsonInclude]
    [JsonPropertyName("PCTColor")]
    public ConfigColor PCTColor { get; set; } = new(150f / 255f, 90f / 255f, 144f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("BLUColor")]
    public ConfigColor BLUColor { get; set; } = new(0f / 255f, 185f / 255f, 247f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("THMColor")]
    public ConfigColor THMColor { get; set; } = new(165f / 255f, 121f / 255f, 214f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("ACNColor")]
    public ConfigColor ACNColor { get; set; } = new(45f / 255f, 155f / 255f, 120f / 255f, 1f);

    [JsonInclude]
    [JsonPropertyName("UKNColor")]
    public ConfigColor UKNColor { get; set; } = new(218f / 255f, 157f / 255f, 46f / 255f, 1f);
}