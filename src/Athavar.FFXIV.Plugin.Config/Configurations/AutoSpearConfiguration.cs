// <copyright file="AutoSpearConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;

public sealed class AutoSpearConfiguration : BasicModuleConfig<AutoSpearConfiguration>
{
    [JsonInclude]
    [JsonPropertyName("FishMatchText")]
    public string? FishMatchText { get; set; } = string.Empty;
}