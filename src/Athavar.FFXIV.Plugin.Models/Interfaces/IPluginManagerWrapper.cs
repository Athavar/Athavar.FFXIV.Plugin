// <copyright file="IPluginManagerWrapper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces;

using Athavar.FFXIV.Plugin.Models.DalamudLike;
using Dalamud.Plugin;

public interface IPluginManagerWrapper
{
    /// <summary>
    ///     Gets all installed plugins.
    /// </summary>
    /// <returns>returns a list of installed plugins.</returns>
    IEnumerable<LocalPlugin> GetInstalledPlugins();

    /// <summary>
    ///     Gets the instance of a plugin.
    /// </summary>
    /// <param name="name">The name of a plugin.</param>
    /// <returns>returns the plugin instance object.</returns>
    IDalamudPlugin? GetPluginInstance(string name);
}