// <copyright file="Potency.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class Potency
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EffectType
    {
        DamageAddPotency = 1,
        DamageDoneMultiplier = 2,
        PotencyMultiplier = 3,
        DamageReceivedMultiplier = 4,
        HealDoneMultiplier = 5,
        HealReceiveMultiplier = 6,
    }

    [JsonPropertyName("type")]
    public EffectType Type { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("amountbyte")]
    public byte? AmountByte { get; set; }

    [JsonPropertyName("isstacked")]
    public bool IsStacked { get; set; }

    [JsonPropertyName("limitto")]
    public string LimitTo { get; set; } = string.Empty;

    [JsonPropertyName("limittoactionids")]
    public uint[] LimitToActionIds { get; set; } = null!;

    [JsonPropertyName("limittodamagetype")]
    public DamageType LimitToDamageType { get; set; }

    [JsonPropertyName("limittoactioncategory")]
    public ActionCategory LimitToActionCategory { get; set; }

    [JsonPropertyName("limittozoneid")]
    public uint? LimitToZoneId { get; set; }
}