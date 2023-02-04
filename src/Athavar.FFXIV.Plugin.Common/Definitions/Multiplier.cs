// <copyright file="Multiplier.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

public class Multiplier
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MultiplierType
    {
        CriticalHit = 1,
        DirectHit = 2,
        CriticalReceived = 3,
    }

    [JsonPropertyName("type")]
    public MultiplierType Type { get; set; }

    [JsonPropertyName("amount")]
    public uint Amount { get; set; }

    [JsonPropertyName("limitto")]
    public string LimitTo { get; set; } = string.Empty;

    [JsonPropertyName("limittoactionids")]
    public uint[] LimitToActionIds { get; set; } = null!;
}