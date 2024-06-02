// <copyright file="Equipment.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Models.Constants;

internal sealed class Equipment
{
    [JsonPropertyName("MainHand")]
    public Item MainHand { get; set; } = new();

    [JsonPropertyName("OffHand")]
    public Item OffHand { get; set; } = new();

    [JsonPropertyName("Head")]
    public Item Head { get; set; } = new();

    [JsonPropertyName("Body")]
    public Item Body { get; set; } = new();

    [JsonPropertyName("Hands")]
    public Item Hands { get; set; } = new();

    [JsonPropertyName("Legs")]
    public Item Legs { get; set; } = new();

    [JsonPropertyName("Feet")]
    public Item Feet { get; set; } = new();

    [JsonPropertyName("Ears")]
    public Item Ears { get; set; } = new() { ItemId = 4294967158 };

    [JsonPropertyName("Neck")]
    public Item Neck { get; set; } = new() { ItemId = 4294967157 };

    [JsonPropertyName("Wrists")]
    public Item Wrists { get; set; } = new() { ItemId = 4294967156 };

    [JsonPropertyName("RFinger")]
    public Item RFinger { get; set; } = new() { ItemId = 4294967155 };

    [JsonPropertyName("LFinger")]
    public Item LFinger { get; set; } = new() { ItemId = 4294967155 };

    [JsonPropertyName("Hat")]
    public ItemState Hat { get; set; } = new();

    [JsonPropertyName("Visor")]
    public VisorState Visor { get; set; } = new();

    [JsonPropertyName("Weapon")]
    public ItemState Weapon { get; set; } = new();

    public Item? GetSlot(EquipSlot slot)
        => slot switch
        {
            EquipSlot.Head => this.Head,
            EquipSlot.Body => this.Body,
            EquipSlot.Hands => this.Hands,
            EquipSlot.Legs => this.Legs,
            EquipSlot.Feet => this.Feet,
            EquipSlot.MainHand => this.MainHand,
            EquipSlot.OffHand => this.OffHand,
            EquipSlot.Ears => this.Ears,
            EquipSlot.Neck => this.Neck,
            EquipSlot.Wrists => this.Wrists,
            EquipSlot.RFinger => this.RFinger,
            EquipSlot.LFinger => this.LFinger,
            _ => null,
        };
}