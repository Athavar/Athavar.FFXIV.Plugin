// <copyright file="PluginManagerWrapper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.DalamudWrapper;

using System.Reflection;
using Athavar.FFXIV.Plugin.Models.DalamudLike;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Plugin;

/// <summary>
///     Wrapper around the plugin manager from dalamud.
/// </summary>
internal sealed class PluginManagerWrapper : IPluginManagerWrapper
{
    private bool init;
    private Type? servicePluginManagerType;

    private PropertyInfo? propertyInstalledPlugins;

    private object? pluginManagerInstance;

    /// <summary>
    ///     Gets all installed plugins.
    /// </summary>
    /// <returns>returns a list of installed plugins.</returns>
    public IEnumerable<LocalPlugin> GetInstalledPlugins()
    {
        this.Init();
        dynamic value = this.propertyInstalledPlugins?.GetValue(this.pluginManagerInstance) ?? throw new Exception("Fail to access InstalledPlugins of PluginManager");

        foreach (var subItem in value)
        {
            yield return new LocalPluginWrapper(subItem);
        }
    }

    /// <summary>
    ///     Gets the instance of a plugin.
    /// </summary>
    /// <param name="name">The name of a plugin.</param>
    /// <returns>returns the plugin instance object.</returns>
    public IDalamudPlugin? GetPluginInstance(string name) => this.GetInstalledPlugins().Where(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).Select(p => p.Instance).SingleOrDefault();

    private void Init()
    {
        if (this.init)
        {
            return;
        }

        this.servicePluginManagerType = DalamudServiceWrapper.GetDalamudAssembly().GetType("Dalamud.Plugin.Internal.PluginManager") ?? throw new Exception("Fail to get type of Dalamud.Plugin.Internal.PluginManager");

        this.propertyInstalledPlugins = this.servicePluginManagerType.GetProperty("InstalledPlugins") ?? throw new Exception("Fail to get PropertyInfo of InstalledPlugins");

        this.pluginManagerInstance = DalamudServiceWrapper.GetServiceInstance(this.servicePluginManagerType) ?? throw new Exception("Fail to get instance of Dalamud.Plugin.Internal.PluginManager");
        this.init = true;
    }
}