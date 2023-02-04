// <copyright file="DamageShield.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public class DamageShield
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DamageShieldType
    {
        TargetHpPercent = 1,
        Potency = 2,
        HealPercent = 3,
    }

    public DamageShieldType Type { get; set; }

    public double Amount { get; set; }
}