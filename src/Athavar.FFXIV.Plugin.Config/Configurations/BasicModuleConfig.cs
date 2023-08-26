// <copyright file="BasicModuleConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;
using Dalamud.Plugin;

/// <summary>
///     Basic module Configuration.
/// </summary>
[Serializable]
public abstract class BasicModuleConfig
{
    internal BasicModuleConfig()
    {
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the plugin functionality is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the plugin functionality is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("TabEnabled")]
    public bool TabEnabled { get; set; } = true;

    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    internal DalamudPluginInterface? Pi { get; set; }

    public abstract void Save(bool instant = false);

    /// <summary>
    ///     Setup <see cref="InstancinatorConfiguration"/>.
    /// </summary>
    /// <param name="conf">The <see cref="Configuration"/>.</param>
    internal void Setup(Configuration conf)
    {
        this.Pi = conf.Pi;
        this.Save(true);
    }
}