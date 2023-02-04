// <copyright file="HealEntry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public class HealEntry
{
    [JsonPropertyName("potency")]
    public uint Potency { get; set; }

    [JsonPropertyName("targetindex")]
    public byte? TargetIndex { get; set; }
}