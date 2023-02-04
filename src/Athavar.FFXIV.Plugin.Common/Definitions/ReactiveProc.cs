// <copyright file="ReactiveProc.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public class ReactiveProc
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
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