// <copyright file="InstancinatorConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;

/// <summary>
///     Instancinator Module configuration.
/// </summary>
public sealed class InstancinatorConfiguration : BasicModuleConfig<InstancinatorConfiguration>
{
    [JsonInclude]
    [JsonPropertyName("ExtraDelay")]
    public int ExtraDelay { get; set; }
}