// <copyright file="TimeProc.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class TimeProc
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TickType
    {
        DoT = 1,
        HoT = 2,
        GroundDamage = 3,
        GroundHeal = 4,
    }

    [JsonPropertyName("type")]
    public TickType Type { get; set; }

    [JsonPropertyName("potency")]
    public uint Potency { get; set; }

    [JsonPropertyName("damagetype")]
    public DamageType DamageType { get; set; }

    [JsonPropertyName("maxticks")]
    public byte MaxTicks { get; set; }
}