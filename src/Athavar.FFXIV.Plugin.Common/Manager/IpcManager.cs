// <copyright file="IpcManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Models.Constants;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

/// <summary>
///     IPC Manager. Currently no longer used with changes in Dalamud v9.
/// </summary>
internal sealed class IpcManager : IIpcManager, IDisposable
{
    private const string GlamourerAssemblyName = "Glamourer";

    private readonly IDalamudServices dalamudServices;
    private readonly IPluginMonitorService pluginMonitorService;
    private readonly IPluginLogger logger;

    private ICallGateSubscriber<(int Breaking, int Features)> glamourerApiVersionsSubscriber;
    private ICallGateSubscriber<IPlayerCharacter?, byte, ulong, byte, uint, int>? glamourerSetItemOnceSubscriber;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IpcManager"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="pluginMonitorService"><see cref="IPluginMonitorService"/> added by DI.</param>
    public IpcManager(IDalamudServices dalamudServices, IPluginMonitorService pluginMonitorService)
    {
        this.dalamudServices = dalamudServices;
        this.pluginMonitorService = pluginMonitorService;
        this.logger = dalamudServices.PluginLogger;
        this.Initialize();
        pluginMonitorService.LoadingStateHasChanged += this.OnPluginLoadingStateHasChanged;
        this.UpdateActivePluginState();
    }

    /// <inheritdoc/>
    public (int Breaking, int Features) GlamourerApiVersion
    {
        get
        {
            try
            {
                this.GlamourerEnabled = true;
                return this.glamourerApiVersionsSubscriber.InvokeFunc();
            }
            catch (IpcNotReadyError)
            {
                this.GlamourerEnabled = false;
                return (-1, -1);
            }
            catch
            {
                this.GlamourerEnabled = false;
                return (-1, -1);
            }
        }
    }

    /// <inheritdoc/>
    public bool GlamourerEnabled { get; private set; }

    public void Dispose()
    {
    }

    /// <inheritdoc/>
    public int SetItem(IPlayerCharacter? character, EquipSlot slot, ulong itemId, byte stainId, uint key)
    {
        if (this.GlamourerApiVersion.Breaking != 0)
        {
            return -1;
        }

        return this.glamourerSetItemOnceSubscriber?.InvokeFunc(character, (byte)slot, itemId, stainId, key) ?? 0;
    }

    public void UpdateActivePluginState()
    {
        this.GlamourerEnabled = this.pluginMonitorService.IsLoaded(GlamourerAssemblyName);
    }

    private void OnPluginLoadingStateHasChanged(string name, bool state, IExposedPlugin? plugin)
    {
        if (name == GlamourerAssemblyName)
        {
            this.GlamourerEnabled = state;
        }
    }

    [MemberNotNull(nameof(glamourerApiVersionsSubscriber))]
    [MemberNotNull(nameof(glamourerSetItemOnceSubscriber))]
    private void Initialize()
    {
        this.glamourerApiVersionsSubscriber = this.dalamudServices.PluginInterface.GetIpcSubscriber<(int, int)>("Glamourer.ApiVersions");
        this.glamourerSetItemOnceSubscriber = this.dalamudServices.PluginInterface.GetIpcSubscriber<IPlayerCharacter?, byte, ulong, byte, uint, int>("Glamourer.SetItemOnce");

        this.dalamudServices.PluginInterface.ActivePluginsChanged += this.OnActivePluginsChanged;
    }

    private void OnActivePluginsChanged(PluginListInvalidationKind kind, bool affectedthisplugin) => this.UpdateActivePluginState();
}