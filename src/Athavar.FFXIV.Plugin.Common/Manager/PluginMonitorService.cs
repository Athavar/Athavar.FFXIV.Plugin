// <copyright file="PluginMonitorService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Services;

internal class PluginMonitorService : IDisposable, IPluginMonitorService
{
    private readonly IDalamudServices dalamudServices;
    private readonly IFrameworkManager frameworkManager;

    private readonly Dictionary<string, Version> loadedPlugins = [];

    private DateTimeOffset nextUpdate = DateTimeOffset.MinValue;

    public PluginMonitorService(IDalamudServices dalamudServices, IFrameworkManager frameworkManager)
    {
        this.dalamudServices = dalamudServices;
        this.frameworkManager = frameworkManager;

        this.frameworkManager.Subscribe(this.FrameworkUpdate);
    }

    public event IPluginMonitorService.PluginLoadingStateChanged? LoadingStateHasChanged;

    public bool IsLoaded(string name) => this.loadedPlugins.ContainsKey(name);

    /// <inheritdoc/>
    public void Dispose()
    {
        this.frameworkManager.Unsubscribe(this.FrameworkUpdate);
    }

    private void FrameworkUpdate(IFramework framework)
    {
        if (this.nextUpdate > DateTimeOffset.UtcNow)
        {
            return;
        }

        this.nextUpdate = DateTimeOffset.UtcNow.AddSeconds(1);

        Dictionary<string, Version> tmp = new(this.loadedPlugins);
        foreach (var exposedPlugin in this.dalamudServices.PluginInterface.InstalledPlugins)
        {
            var name = exposedPlugin.InternalName;
            if (exposedPlugin.IsLoaded)
            {
                if (!tmp.Remove(name, out var version))
                {
                    this.loadedPlugins.Add(name, exposedPlugin.Version);
                    this.LoadingStateHasChanged?.Invoke(name, true, exposedPlugin);
                    continue;
                }

                if (exposedPlugin.Version != version)
                {
                    this.loadedPlugins[name] = exposedPlugin.Version;
                    this.LoadingStateHasChanged?.Invoke(name, false, exposedPlugin);
                    this.LoadingStateHasChanged?.Invoke(name, true, exposedPlugin);
                }
            }
            else
            {
                if (tmp.Remove(name, out _))
                {
                    this.loadedPlugins.Remove(name);
                    this.LoadingStateHasChanged?.Invoke(name, false, exposedPlugin);
                }
            }
        }

        foreach (var (name, _) in tmp)
        {
            this.LoadingStateHasChanged?.Invoke(name, false, null);
        }
    }
}