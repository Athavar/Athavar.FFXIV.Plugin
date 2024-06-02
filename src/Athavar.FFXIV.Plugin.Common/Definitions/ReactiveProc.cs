// <copyright file="ReactiveProc.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public sealed class ReactiveProc
{
    [JsonConverter(typeof(JsonStringEnumConverter<ReactiveProcType>))]
    public enum ReactiveProcType
    {
        DamageOnDamageReceived = 1,
        HealOnDamageDealt = 2,
        HealOnDamageReceived = 3,
        HealOnHealCast = 4,
    }

    [JsonPropertyName("type")]
    public ReactiveProcType Type { get; set; }

    [JsonPropertyName("amount")]
    public uint? Amount { get; set; }
}