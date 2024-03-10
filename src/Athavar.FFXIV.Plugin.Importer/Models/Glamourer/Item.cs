// <copyright file="Item.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;

internal class Item
{
    [JsonPropertyName("ItemId")]
    public uint ItemId { get; set; }

    [JsonPropertyName("Stain")]
    public uint Stain { get; set; }

    [JsonPropertyName("Crest")]
    public bool Crest { get; set; } = false;

    [JsonPropertyName("Apply")]
    public bool Apply { get; set; } = false;

    [JsonPropertyName("ApplyStain")]
    public bool ApplyStain { get; set; } = false;

    [JsonPropertyName("ApplyCrest")]
    public bool ApplyCrest { get; set; } = false;
}