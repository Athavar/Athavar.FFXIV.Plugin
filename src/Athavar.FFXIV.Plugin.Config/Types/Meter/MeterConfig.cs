// <copyright file="MeterConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Text.Json.Serialization;

public sealed class MeterConfig : BaseConfig
{
    [JsonInclude]
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonInclude]
    [JsonPropertyName("GeneralConfig")]
    public GeneralConfig GeneralConfig { get; set; } = new();

    [JsonInclude]
    [JsonPropertyName("HeaderConfig")]
    public HeaderConfig HeaderConfig { get; set; } = new();

    [JsonInclude]
    [JsonPropertyName("BarConfig")]
    public BarConfig BarConfig { get; set; } = new();

    [JsonInclude]
    [JsonPropertyName("BarColorsConfig")]
    public BarColorsConfig BarColorsConfig { get; set; } = new();

    [JsonInclude]
    [JsonPropertyName("VisibilityConfig")]
    public VisibilityConfig VisibilityConfig { get; set; } = new();
}