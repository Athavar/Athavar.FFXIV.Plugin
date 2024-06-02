// <copyright file="Links.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

internal sealed class Links
{
    [JsonPropertyName("Before")]
    public Collection<UnImplemented> Before { get; set; } = new();

    [JsonPropertyName("After")]
    public Collection<UnImplemented> After { get; set; } = new();
}