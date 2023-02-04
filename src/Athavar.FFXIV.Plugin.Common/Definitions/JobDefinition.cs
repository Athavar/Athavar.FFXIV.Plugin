// <copyright file="JobDefinition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Definitions.Converter;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public class JobDefinition
{
    [JsonPropertyName("job")]
    public string Job { get; set; } = string.Empty;

    [JsonPropertyName("actions")]
    [JsonConverter(typeof(JsonActionConverter))]
    public Dictionary<uint, Action> Actions { get; set; } = new();

    [JsonPropertyName("statuseffects")]
    [JsonConverter(typeof(JsonStatusEffectConverter))]
    public Dictionary<uint, StatusEffect> Statuseffects { get; set; } = new();
}