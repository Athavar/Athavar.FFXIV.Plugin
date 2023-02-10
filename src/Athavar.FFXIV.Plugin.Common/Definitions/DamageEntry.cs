// <copyright file="DamageEntry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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