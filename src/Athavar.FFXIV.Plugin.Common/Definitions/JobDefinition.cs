// <copyright file="JobDefinition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Definitions.Converter;

[JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
public sealed class JobDefinition
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