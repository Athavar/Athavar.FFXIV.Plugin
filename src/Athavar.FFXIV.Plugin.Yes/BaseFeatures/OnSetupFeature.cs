// <copyright file="OnSetupFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.BaseFeatures;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

/// <summary>
///     An abstract that hooks OnSetup to provide a feature.
/// </summary>
internal abstract class OnSetupFeature : IBaseFeature
{
    protected readonly YesModule module;
    private readonly AddonEvent trigger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OnSetupFeature"/> class.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <param name="trigger">The event that triggers the feature.</param>
    protected OnSetupFeature(YesModule module, AddonEvent trigger = AddonEvent.PostRequestedUpdate)
    {
        this.module = module;
        this.trigger = trigger;
        module.DalamudServices.AddonLifecycle.RegisterListener(this.trigger, this.AddonName, this.TriggerHandler);
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
    public void Dispose() => this.module.DalamudServices.AddonLifecycle.UnregisterListener(this.trigger, this.AddonName, this.TriggerHandler);

    /// <summary>
    ///     A method that is run within the OnSetup detour.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="addonEvent">Addon trigger event.</param>
    protected abstract void OnSetupImpl(IntPtr addon, AddonEvent addonEvent);

    protected void TriggerHandler(AddonEvent type, AddonArgs args)
    {
        if (this.trigger != AddonEvent.PostUpdate)
        {
            this.module.Logger.Debug($"Addon{this.AddonName}.OnSetup");
        }

        if (!this.module.MC.ModuleEnabled || this.module.DisableKeyPressed)
        {
            return;
        }

        try
        {
            this.OnSetupImpl(args.Addon, type);
        }
        catch (Exception ex)
        {
            this.module.Logger.Error(ex, "Don't crash the game");
        }
    }
}