// <copyright file="OnSetupFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.BaseFeatures;

using Dalamud.Hooking;

/// <summary>
///     An abstract that hooks OnSetup to provide a feature.
/// </summary>
internal abstract class OnSetupFeature : IBaseFeature
{
    protected readonly YesModule module;
    private readonly Hook<OnSetupDelegate> onSetupHook;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OnSetupFeature"/> class.
    /// </summary>
    /// <param name="onSetupSig">Signature to the OnSetup method.</param>
    /// <param name="module">The module.</param>
    public OnSetupFeature(string onSetupSig, YesModule module)
    {
        this.module = module;
        this.onSetupHook = this.module.DalamudServices.GameInteropProvider.HookFromSignature(onSetupSig, (OnSetupDelegate)this.OnSetupDetour);
        this.onSetupHook.Enable();
    }

    /// <summary>
    ///     A delegate matching AtkUnitBase.OnSetup.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Unused for our purposes.</param>
    /// <param name="data">Data address.</param>
    /// <returns>The addon address.</returns>
    internal delegate nint OnSetupDelegate(nint addon, uint a2, nint data);

    /// <summary>
    ///     Gets the <see cref="YesConfiguration"/>.
    /// </summary>
    protected YesConfiguration Configuration => this.module.MC;

    /// <summary>
    ///     Gets the name of the addon being hooked.
    /// </summary>
    protected abstract string AddonName { get; }

    /// <inheritdoc/>
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
    protected abstract void OnSetupImpl(nint addon, uint a2, nint data);

    private nint OnSetupDetour(nint addon, uint a2, nint data)
    {
        this.module.Logger.Debug($"Addon{this.AddonName}.OnSetup");
        var result = this.onSetupHook.Original(addon, a2, data);

        if (!this.module.MC.ModuleEnabled || this.module.DisableKeyPressed)
        {
            return result;
        }

        if (addon == nint.Zero)
        {
            return result;
        }

        try
        {
            this.OnSetupImpl(addon, a2, data);
        }
        catch (Exception ex)
        {
            this.module.Logger.Error(ex, "Don't crash the game");
        }

        return result;
    }
}