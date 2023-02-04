// <copyright file="DamageEntry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public class DamageEntry
{
    [JsonPropertyName("potency")]
    public uint Potency { get; set; }

    [JsonPropertyName("combo")]
    public uint? Combo { get; set; }

    [JsonPropertyName("targetindex")]
    public byte? TargetIndex { get; set; }
}