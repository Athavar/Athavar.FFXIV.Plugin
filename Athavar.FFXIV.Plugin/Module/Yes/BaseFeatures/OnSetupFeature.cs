// <copyright file="OnSetupFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

using System;
using Dalamud.Hooking;
using Dalamud.Logging;

/// <summary>
///     An abstract that hooks OnSetup to provide a feature.
/// </summary>
internal abstract class OnSetupFeature : IBaseFeature
{
    private readonly YesModule module;
    private readonly Hook<OnSetupDelegate> onSetupHook;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OnSetupFeature" /> class.
    /// </summary>
    /// <param name="onSetupSig">Signature to the OnSetup method.</param>
    /// <param name="module">The module.</param>
    public OnSetupFeature(string onSetupSig, YesModule module)
    {
        this.module = module;
        this.HookAddress = module.DalamudServices.SigScanner.ScanText(onSetupSig);
        this.onSetupHook = new Hook<OnSetupDelegate>(this.HookAddress, this.OnSetupDetour);
        this.onSetupHook.Enable();
    }

    /// <summary>
    ///     A delegate matching AtkUnitBase.OnSetup.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Unused for our purposes.</param>
    /// <param name="data">Data address.</param>
    /// <returns>The addon address.</returns>
    internal delegate IntPtr OnSetupDelegate(IntPtr addon, uint a2, IntPtr data);

    /// <summary>
    ///     Gets the <see cref="YesConfiguration" />.
    /// </summary>
    protected YesConfiguration Configuration => this.module.Configuration;

    /// <summary>
    ///     Gets the name of the addon being hooked.
    /// </summary>
    protected abstract string AddonName { get; }

    /// <summary>
    ///     Gets the address of the addon Update function.
    /// </summary>
    protected IntPtr HookAddress { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.onSetupHook?.Disable();
        this.onSetupHook?.Dispose();
    }

    /// <summary>
    ///     A method that is run within the OnSetup detour.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Unknown paramater.</param>
    /// <param name="data">Setup data address.</param>
    protected abstract void OnSetupImpl(IntPtr addon, uint a2, IntPtr data);

    private IntPtr OnSetupDetour(IntPtr addon, uint a2, IntPtr data)
    {
        PluginLog.Debug($"Addon{this.AddonName}.OnSetup");
        var result = this.onSetupHook.Original(addon, a2, data);

        if (!this.module.Configuration.Enabled || this.module.DisableKeyPressed)
        {
            return result;
        }

        if (addon == IntPtr.Zero)
        {
            return result;
        }

        try
        {
            this.OnSetupImpl(addon, a2, data);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Don't crash the game");
        }

        return result;
    }
}