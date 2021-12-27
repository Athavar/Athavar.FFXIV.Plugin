// <copyright file="UpdateFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

using System;
using Dalamud.Hooking;
using Dalamud.Logging;

/// <summary>
///     An abstract that hooks Update to provide a feature.
/// </summary>
internal abstract class UpdateFeature : IBaseFeature
{
    private readonly YesModule module;
    private readonly Hook<UpdateDelegate> updateHook;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateFeature" /> class.
    /// </summary>
    /// <param name="updateAddress">Address to the Update method.</param>
    /// <param name="module">The module.</param>
    public UpdateFeature(IntPtr updateAddress, YesModule module)
    {
        this.module = module;
        this.updateHook = new Hook<UpdateDelegate>(updateAddress, this.UpdateDetour);
        this.updateHook.Enable();
    }

    /// <summary>
    ///     A delegate matching AtkUnitBase.OnSetup.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Param2.</param>
    /// <param name="a3">Param3.</param>
    internal delegate void UpdateDelegate(IntPtr addon, IntPtr a2, IntPtr a3);

    /// <summary>
    ///     Gets the <see cref="YesConfiguration" />.
    /// </summary>
    protected YesConfiguration Configuration => this.module.Configuration;

    /// <summary>
    ///     Gets the name of the addon being hooked.
    /// </summary>
    protected abstract string AddonName { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.updateHook?.Disable();
        this.updateHook?.Dispose();
    }

    /// <summary>
    ///     A method that is run within the Update detour.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Unknown parameter 2.</param>
    /// <param name="a3">Unknown parameter 3.</param>
    protected abstract void UpdateImpl(IntPtr addon, IntPtr a2, IntPtr a3);

    private void UpdateDetour(IntPtr addon, IntPtr a2, IntPtr a3)
    {
        // Update is noisy, dont echo here.
        // PluginLog.Debug($"Addon{this.AddonName}.Update");
        this.updateHook.Original(addon, a2, a3);

        if (!this.Configuration.Enabled || this.module.DisableKeyPressed)
        {
            return;
        }

        if (addon == IntPtr.Zero)
        {
            return;
        }

        try
        {
            this.UpdateImpl(addon, a2, a3);
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Don't crash the game");
        }
    }
}