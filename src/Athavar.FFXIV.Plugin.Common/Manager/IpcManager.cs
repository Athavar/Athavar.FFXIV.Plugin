// <copyright file="IpcManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

/// <summary>
///     IPC Manager. Currently no longer used with changes in Dalamud v9.
/// </summary>
internal sealed class IpcManager : IIpcManager, IDisposable
{
    private readonly IPluginLogger logger;

    private ICallGateSubscriber<(int Breaking, int Features)> penumbraApiVersionsSubscriber;
    private ICallGateSubscriber<string, string>? penumbraResolveInterfacePathSubscriber;

    private ICallGateSubscriber<object> penumbraInitializedSubscriber;
    private ICallGateSubscriber<object> penumbraDisposedSubscriber;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IpcManager"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    public IpcManager(IDalamudServices dalamudServices)
    {
        this.logger = dalamudServices.PluginLogger;
        this.Initialize(dalamudServices);
    }

    /// <inheritdoc/>
    public event EventHandler? PenumbraStatusChanged;

    /// <inheritdoc/>
    public (int Breaking, int Features) PenumbraApiVersion
    {
        get
        {
            try
            {
                return this.penumbraApiVersionsSubscriber.InvokeFunc();
            }
            catch (IpcNotReadyError)
            {
                return (0, 0);
            }
            catch
            {
                return (-1, -1);
            }
        }
    }

    /// <inheritdoc/>
    public bool PenumbraEnabled { get; private set; }

    /// <inheritdoc/>
    public string ResolvePenumbraPath(string path)
    {
        if (!this.PenumbraEnabled || this.penumbraResolveInterfacePathSubscriber is null)
        {
            return path;
        }

        try
        {
            var resolvedPath = this.penumbraResolveInterfacePathSubscriber.InvokeFunc(path);
            return resolvedPath;
        }
        catch (IpcNotReadyError)
        {
            this.logger.Information("IpcNotReadyError");
            return path;
        }
        catch (Exception ex)
        {
            this.logger.Error(ex, "Failed while try to use Penumbra IPC. Disable integration");
            this.PenumbraEnabled = false;
            return path;
        }
    }

    public void Dispose()
    {
        this.penumbraInitializedSubscriber.Unsubscribe(this.EnablePenumbraApi);
        this.penumbraDisposedSubscriber.Unsubscribe(this.DisablePenumbraApi);
    }

    [MemberNotNull(nameof(penumbraInitializedSubscriber))]
    [MemberNotNull(nameof(penumbraDisposedSubscriber))]
    [MemberNotNull(nameof(penumbraApiVersionsSubscriber))]
    [MemberNotNull(nameof(penumbraResolveInterfacePathSubscriber))]
    private void Initialize(IDalamudServices dalamudServices)
    {
        this.penumbraApiVersionsSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<(int, int)>("Penumbra.ApiVersions");
        this.penumbraResolveInterfacePathSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<string, string>("Penumbra.ResolveInterfacePath");
        this.penumbraInitializedSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<object>("Penumbra.Initialized");
        this.penumbraDisposedSubscriber = dalamudServices.PluginInterface.GetIpcSubscriber<object>("Penumbra.Disposed");

        this.penumbraInitializedSubscriber.Subscribe(this.EnablePenumbraApi);
        this.penumbraDisposedSubscriber.Subscribe(this.DisablePenumbraApi);

        this.EnablePenumbraApi();
    }

    private void EnablePenumbraApi()
    {
        if (this.PenumbraApiVersion.Breaking != 4)
        {
            return;
        }

        this.PenumbraEnabled = true;
    }

    private void DisablePenumbraApi()
    {
        if (!this.PenumbraEnabled)
        {
            return;
        }

        this.PenumbraEnabled = false;
    }
}