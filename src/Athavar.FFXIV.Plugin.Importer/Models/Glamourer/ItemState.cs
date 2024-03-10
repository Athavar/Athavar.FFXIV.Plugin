// <copyright file="ItemState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;

internal class ItemState
{
    [JsonPropertyName("Show")]
    public bool Show { get; set; }

    [JsonPropertyName("Apply")]
    public bool Apply { get; set; }
}