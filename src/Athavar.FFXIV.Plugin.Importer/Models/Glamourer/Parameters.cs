// <copyright file="Parameters.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;

internal class Parameters
{
    [JsonPropertyName("FacePaintUvMultiplier")]
    public ValueNumber FacePaintUvMultiplier { get; set; } = new();

    [JsonPropertyName("FacePaintUvOffset")]
    public ValueNumber FacePaintUvOffset { get; set; } = new();

    [JsonPropertyName("MuscleTone")]
    public ValuePercentage MuscleTone { get; set; } = new();

    [JsonPropertyName("SkinDiffuse")]
    public Color SkinDiffuse { get; set; } = new();

    [JsonPropertyName("SkinSpecular")]
    public Color SkinSpecular { get; set; } = new();

    [JsonPropertyName("HairDiffuse")]
    public Color HairDiffuse { get; set; } = new();

    [JsonPropertyName("HairSpecular")]
    public Color HairSpecular { get; set; } = new();

    [JsonPropertyName("HairHighlight")]
    public Color HairHighlight { get; set; } = new();

    [JsonPropertyName("LeftEye")]
    public Color LeftEye { get; set; } = new();

    [JsonPropertyName("RightEye")]
    public Color RightEye { get; set; } = new();

    [JsonPropertyName("FeatureColor")]
    public Color FeatureColor { get; set; } = new();

    [JsonPropertyName("LipDiffuse")]
    public ColorAlpha LipDiffuse { get; set; } = new();

    [JsonPropertyName("DecalColor")]
    public ColorAlpha DecalColor { get; set; } = new();
}