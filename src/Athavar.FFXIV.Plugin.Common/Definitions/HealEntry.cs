// <copyright file="HealEntry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public sealed class HealEntry
{
    [JsonPropertyName("potency")]
    public uint Potency { get; set; }

    [JsonPropertyName("targetindex")]
    public byte? TargetIndex { get; set; }
}