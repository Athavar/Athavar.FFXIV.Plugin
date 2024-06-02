// <copyright file="ValueBool.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Importer.Models.Glamourer;

using System.Text.Json.Serialization;

internal sealed class ValueBool
{
    [JsonPropertyName("Value")]
    public bool Value { get; set; }

    [JsonPropertyName("Apply")]
    public bool Apply { get; set; } = false;
}