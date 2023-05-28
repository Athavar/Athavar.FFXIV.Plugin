// <copyright file="DpsConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

public sealed class DpsConfiguration : BasicModuleConfig<DpsConfiguration>
{
    [JsonInclude]
    [JsonPropertyName("Meters")]
    public List<MeterConfig> Meters { get; set; } = new();

    [JsonInclude]
    [JsonPropertyName("PartyFilter")]
    public PartyType PartyFilter { get; set; } = PartyType.Alliance;

    [JsonInclude]
    [JsonPropertyName("TextRefreshInterval")]
    public int TextRefreshInterval { get; set; } = 200;
}