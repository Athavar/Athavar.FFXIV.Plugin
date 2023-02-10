// <copyright file="EnablePeepingTomPvP.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

using System.Reflection;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Logging;
using Dalamud.Plugin;

/// <summary>
///     Cheat to also enable peeping tom in pvp areas.
/// </summary>
internal class EnablePeepingTomPvP : ICheat
{
    private readonly IDalamudServices dalamudServices;
    private readonly PluginManagerWrapper pluginManagerWrapper;

    private PropertyInfo? pvpField;
    private IDalamudPlugin? plugin;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EnablePeepingTomPvP" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="pluginManagerWrapper"><see cref="pluginManagerWrapper" /> added by DI.</param>
    public EnablePeepingTomPvP(IDalamudServices dalamudServices, PluginManagerWrapper pluginManagerWrapper)
    {
        this.dalamudServices = dalamudServices;
        this.pluginManagerWrapper = pluginManagerWrapper;
    }

    /// <inheritdoc />
    public bool Enabled => this.dalamudServices.PluginInterface.PluginNames.Contains("Peeping Tom");

    /// <inheritdoc />
    public bool OnEnabled()
    {
        var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "PeepingTom");
        var type = assembly?.GetType("PeepingTom.PeepingTomPlugin");
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

        this.dalamudServices.ClientState.TerritoryChanged += this.OnTerritoryChange;
        return true;
    }

    /// <inheritdoc />
    public void OnDisabled()
    {
        this.pvpField = null;
        this.plugin = null;

        this.dalamudServices.ClientState.TerritoryChanged -= this.OnTerritoryChange;
    }

    private void OnTerritoryChange(object? sender, ushort e)
    {
        try
        {
            Task.Delay(50);
            this.pvpField?.SetValue(this.plugin, false);
        }
        catch (KeyNotFoundException)
        {
            PluginLog.Warning("Could not get territory for current zone");
        }
    }
}