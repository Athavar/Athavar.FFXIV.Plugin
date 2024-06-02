// <copyright file="PluginManagerWrapper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

using System.Reflection;
using Dalamud.Plugin;

/// <summary>
///     Wrapper around the plugin manager from dalamud.
/// </summary>
internal sealed class PluginManagerWrapper
{
    private bool init;
    private Type? servicePluginManagerType;

    private PropertyInfo? propertyInstalledPlugins;

    private object? pluginManagerInstance;

    /// <summary>
    ///     Values representing plugin load state.
    /// </summary>
    internal enum PluginState
    {
        /// <summary>
        ///     Plugin is defined, but unloaded.
        /// </summary>
        Unloaded,

        /// <summary>
        ///     Plugin has thrown an error during unload.
        /// </summary>
        UnloadError,

        /// <summary>
        ///     Currently unloading.
        /// </summary>
        Unloading,

        /// <summary>
        ///     Load is successful.
        /// </summary>
        Loaded,

        /// <summary>
        ///     Plugin has thrown an error during loading.
        /// </summary>
        LoadError,

        /// <summary>
        ///     Currently loading.
        /// </summary>
        Loading,

        /// <summary>
        ///     This plugin couldn't load one of its dependencies.
        /// </summary>
        DependencyResolutionFailed,
    }

    /// <summary>
    ///     Gets all installed plugins.
    /// </summary>
    /// <returns>returns a list of installed plugins.</returns>
    public IEnumerable<LocalPluginWrapper> GetInstalledPlugins()
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

    /// <summary>
    ///     Wrapper around the internal LocalPlugin of Dalamud.
    /// </summary>
    internal sealed class LocalPluginWrapper
    {
        private static Type? localPluginType;
        private readonly dynamic instance;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LocalPluginWrapper" /> class.
        /// </summary>
        /// <param name="instance">The un-accessible LocalPlugin object.</param>
        internal LocalPluginWrapper(dynamic instance) => this.instance = instance;

        /// <summary>
        ///     Gets the name of the plugin.
        /// </summary>
        public string Name => (string)(Type.GetProperty("Name")?.GetValue(this.instance) ?? string.Empty);

        /// <summary>
        ///     Gets the plugin instance.
        /// </summary>
        public IDalamudPlugin? Instance => (IDalamudPlugin?)Type.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField)?.GetValue(this.instance);

        /// <summary>
        ///     Gets the assembly name of the plugin.
        /// </summary>
        public AssemblyName? AssemblyName => (AssemblyName?)Type.GetField("AssemblyName")?.GetValue(this.instance);

        /// <summary>
        ///     Gets the state of the plugin.
        /// </summary>
        public PluginState State => (PluginState)(Type.GetField("State")?.GetValue(this.instance) ?? PluginState.DependencyResolutionFailed);

        private static Type Type => localPluginType ??= DalamudServiceWrapper.GetDalamudAssembly().GetType("Dalamud.Plugin.Internal.Types.LocalPlugin") ?? throw new Exception("Fail to find Type Dalamud.Plugin.Internal.Types.LocalPlugin");
    }
}