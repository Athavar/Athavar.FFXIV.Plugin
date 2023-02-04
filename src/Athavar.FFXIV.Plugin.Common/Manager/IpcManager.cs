// <copyright file="IpcManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Dalamud.Logging;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Ipc.Exceptions;

internal class IpcManager : IIpcManager, IDisposable
{
    private ICallGateSubscriber<(int Breaking, int Features)> penumbraApiVersionsSubscriber;
    private ICallGateSubscriber<string, string>? penumbraResolveInterfacePathSubscriber;

    private ICallGateSubscriber<object> penumbraInitializedSubscriber;
    private ICallGateSubscriber<object> penumbraDisposedSubscriber;

    /// <summary>
    ///     Initializes a new instance of the <see cref="IpcManager" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public IpcManager(IDalamudServices dalamudServices) => this.Initialize(dalamudServices);

    /// <inheritdoc />
    public event EventHandler? PenumbraStatusChanged;

    /// <inheritdoc />
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

    /// <inheritdoc />
    public bool PenumbraEnabled { get; private set; }

    /// <inheritdoc />
    public string ResolvePenumbraPath(string path)
    {
        if (!this.PenumbraEnabled || this.penumbraResolveInterfacePathSubscriber is null)
        {
            PluginLog.Information("Pen State {0} {1}", !this.PenumbraEnabled, this.penumbraResolveInterfacePathSubscriber is null);
            return path;
        }

        try
        {
            var resolvedPath = this.penumbraResolveInterfacePathSubscriber.InvokeFunc(path);
            PluginLog.Information("Resolve Path {0} -> {1}", path, resolvedPath);
            return resolvedPath;
        }
        catch (IpcNotReadyError)
        {
            PluginLog.Information("IpcNotReadyError");
            return path;
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Failed while try to use Penumbra IPC. Disable integration");
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