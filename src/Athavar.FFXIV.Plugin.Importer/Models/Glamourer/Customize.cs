// <copyright file="Customize.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;

internal sealed class Customize
{
    [JsonPropertyName("ModelId")]
    public uint ModelId { get; set; }

    [JsonPropertyName("Race")]
    public ValueInt Race { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Gender")]
    public ValueInt Gender { get; set; } = new() { Value = 0 };

    [JsonPropertyName("BodyType")]
    public ValueInt BodyType { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Height")]
    public ValueInt Height { get; set; } = new() { Value = 50 };

    [JsonPropertyName("Clan")]
    public ValueInt Clan { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Face")]
    public ValueInt Face { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Hairstyle")]
    public ValueInt Hairstyle { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Highlights")]
    public ValueInt Highlights { get; set; } = new() { Value = 0 };

    [JsonPropertyName("SkinColor")]
    public ValueInt SkinColor { get; set; } = new() { Value = 1 };

    [JsonPropertyName("EyeColorRight")]
    public ValueInt EyeColorRight { get; set; } = new() { Value = 1 };

    [JsonPropertyName("HairColor")]
    public ValueInt HairColor { get; set; } = new() { Value = 0 };

    [JsonPropertyName("HighlightsColor")]
    public ValueInt HighlightsColor { get; set; } = new() { Value = 1 };

    [JsonPropertyName("FacialFeature1")]
    public ValueInt FacialFeature1 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacialFeature2")]
    public ValueInt FacialFeature2 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacialFeature3")]
    public ValueInt FacialFeature3 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacialFeature4")]
    public ValueInt FacialFeature4 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacialFeature5")]
    public ValueInt FacialFeature5 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacialFeature6")]
    public ValueInt FacialFeature6 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacialFeature7")]
    public ValueInt FacialFeature7 { get; set; } = new() { Value = 0 };

    [JsonPropertyName("LegacyTattoo")]
    public ValueInt LegacyTattoo { get; set; } = new() { Value = 0 };

    [JsonPropertyName("TattooColor")]
    public ValueInt TattooColor { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Eyebrows")]
    public ValueInt Eyebrows { get; set; } = new() { Value = 1 };

    [JsonPropertyName("EyeColorLeft")]
    public ValueInt EyeColorLeft { get; set; } = new() { Value = 1 };

    [JsonPropertyName("EyeShape")]
    public ValueInt EyeShape { get; set; } = new() { Value = 1 };

    [JsonPropertyName("SmallIris")]
    public ValueInt SmallIris { get; set; } = new() { Value = 0 };

    [JsonPropertyName("Nose")]
    public ValueInt Nose { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Jaw")]
    public ValueInt Jaw { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Mouth")]
    public ValueInt Mouth { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Lipstick")]
    public ValueInt Lipstick { get; set; } = new() { Value = 0 };

    [JsonPropertyName("LipColor")]
    public ValueInt LipColor { get; set; } = new() { Value = 1 };

    [JsonPropertyName("MuscleMass")]
    public ValueInt MuscleMass { get; set; } = new() { Value = 50 };

    [JsonPropertyName("TailShape")]
    public ValueInt TailShape { get; set; } = new() { Value = 1 };

    [JsonPropertyName("BustSize")]
    public ValueInt BustSize { get; set; } = new() { Value = 50 };

    [JsonPropertyName("FacePaint")]
    public ValueInt FacePaint { get; set; } = new() { Value = 1 };

    [JsonPropertyName("FacePaintReversed")]
    public ValueInt FacePaintReversed { get; set; } = new() { Value = 0 };

    [JsonPropertyName("FacePaintColor")]
    public ValueInt FacePaintColor { get; set; } = new() { Value = 1 };

    [JsonPropertyName("Wetness")]
    public ValueBool Wetness { get; set; } = new() { Value = false };
}