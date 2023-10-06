// <copyright file="UpdateFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.BaseFeatures;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

/// <summary>
///     An abstract that hooks Update to provide a feature.
/// </summary>
internal abstract class UpdateFeature : IBaseFeature
{
    private readonly YesModule module;
    private readonly AddonEvent trigger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="UpdateFeature"/> class.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <param name="trigger">The event that triggers the feature.</param>
    protected UpdateFeature(YesModule module, AddonEvent trigger = AddonEvent.PostRequestedUpdate)
    {
        this.module = module;
        this.trigger = trigger;
        module.DalamudServices.AddonLifecycle.RegisterListener(this.trigger, this.AddonName, this.TriggerHandler);
    }

    /// <summary>
    ///     A delegate matching AtkUnitBase.OnSetup.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="a2">Param2.</param>
    /// <param name="a3">Param3.</param>
    internal delegate void UpdateDelegate(nint addon, nint a2, nint a3);

    /// <summary>
    ///     Gets the <see cref="YesConfiguration"/>.
    /// </summary>
    protected YesConfiguration Configuration => this.module.MC;

    /// <summary>
    ///     Gets the name of the addon being hooked.
    /// </summary>
    protected abstract string AddonName { get; }

    /// <inheritdoc/>
    public virtual void Dispose() => this.module.DalamudServices.AddonLifecycle.UnregisterListener(this.trigger, this.AddonName, this.TriggerHandler);

    /// <summary>
    ///     A method that is run within the Update detour.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="addonEvent">Addon Event trigger type.</param>
    protected abstract void UpdateImpl(IntPtr addon, AddonEvent addonEvent);

    protected void TriggerHandler(AddonEvent type, AddonArgs args)
    {
        this.module.Logger.Debug($"Addon{this.AddonName}.Update");
        if (!this.module.MC.ModuleEnabled || this.module.DisableKeyPressed)
        {
            return;
        }

        try
        {
            this.UpdateImpl(args.Addon, type);
        }
        catch (Exception ex)
        {
            this.module.Logger.Error(ex, "Don't crash the game");
        }
    }
}