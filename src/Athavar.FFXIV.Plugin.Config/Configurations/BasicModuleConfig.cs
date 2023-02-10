// <copyright file="BasicModuleConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using Newtonsoft.Json;

/// <summary>
///     Basic module Configuration
/// </summary>
[Serializable]
public abstract class BasicModuleConfig
{
    [JsonIgnore]
    private Configuration? configuration;

    /// <summary>
    ///     Gets or sets a value indicating whether the plugin functionality is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    ///     Save the configuration.
    /// </summary>
    public void Save() => this.configuration?.Save();

    /// <summary>
    ///     Setup <see cref="InstancinatorConfiguration" />.
    /// </summary>
    /// <param name="conf">The <see cref="Configuration" />.</param>
    internal void Setup(Configuration conf) => this.configuration = conf;
}