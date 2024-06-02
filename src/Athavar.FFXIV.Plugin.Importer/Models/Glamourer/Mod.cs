// <copyright file="Mod.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;

internal class Mod
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Directory")]
    public string Directory { get; set; } = string.Empty;

    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; }

    [JsonPropertyName("Priority")]
    public int Priority { get; set; }

    [JsonPropertyName("Settings")]
    public Dictionary<string, string[]> Settings { get; set; } = new();
}