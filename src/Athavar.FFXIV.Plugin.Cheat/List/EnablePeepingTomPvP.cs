// <copyright file="EnablePeepingTomPvP.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Cheat.List;

using System.Reflection;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin;

/// <summary>
///     Cheat to also enable peeping tom in pvp areas.
/// </summary>
internal class EnablePeepingTomPvP : Cheat, IDisposable
{
    private const string AssemblyName = "PeepingTom";

    private readonly IDalamudServices dalamudServices;
    private readonly IPluginMonitorService monitorService;
    private readonly IPluginManagerWrapper pluginManagerWrapper;

    private PropertyInfo? pvpField;
    private IDalamudPlugin? plugin;

    private bool enabled;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EnablePeepingTomPvP"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="monitorService"><see cref="IPluginMonitorService"/> added by DI.</param>
    /// <param name="pluginManagerWrapper"><see cref="pluginManagerWrapper"/> added by DI.</param>
    public EnablePeepingTomPvP(IDalamudServices dalamudServices, IPluginMonitorService monitorService, IPluginManagerWrapper pluginManagerWrapper)
    {
        this.dalamudServices = dalamudServices;
        this.monitorService = monitorService;
        this.pluginManagerWrapper = pluginManagerWrapper;

        monitorService.LoadingStateHasChanged += this.OnPluginLoadingStateHasChanged;
        this.enabled = this.monitorService.IsLoaded(AssemblyName);
    }

    /// <inheritdoc/>
    public override bool Enabled => this.enabled;

    /// <inheritdoc/>
    public override bool OnEnabled()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == AssemblyName);
        var type = assembly?.GetType("PeepingTom.Plugin");
        if (type is null)
        {
            return false;
        }

        this.pvpField = type.GetProperty("InPvp", BindingFlags.Instance | BindingFlags.NonPublic);

        this.plugin = this.pluginManagerWrapper.GetPluginInstance("Peeping Tom");
        if (this.plugin is null)
        {
            return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override void OnDisabled()
    {
        this.pvpField = null;
        this.plugin = null;
    }

    /// <inheritdoc/>
    public override void OnTerritoryChange(ushort e)
        => Task.Run(
            () =>
            {
                try
                {
                    Task.Delay(50);
                    this.pvpField?.SetValue(this.plugin, false);
                }
                catch (KeyNotFoundException)
                {
                    this.dalamudServices.PluginLogger.Warning("Could not get territory for current zone");
                }
            });

    /// <inheritdoc/>
    public void Dispose()
    {
        this.monitorService.LoadingStateHasChanged += this.OnPluginLoadingStateHasChanged;
    }

    private void OnPluginLoadingStateHasChanged(string name, bool state, IExposedPlugin? exposedPlugin)
    {
        if (name == AssemblyName)
        {
            this.enabled = state;
        }
    }
}