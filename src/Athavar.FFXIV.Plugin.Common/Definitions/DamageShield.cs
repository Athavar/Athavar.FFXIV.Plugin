// <copyright file="DamageShield.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public sealed class DamageShield
{
    [JsonConverter(typeof(JsonStringEnumConverter<DamageShieldType>))]
    public enum DamageShieldType
    {
        TargetHpPercent = 1,
        Potency = 2,
        HealPercent = 3,
    }

    public DamageShieldType Type { get; set; }

    public double Amount { get; set; }
}