// <copyright file="OnSetupFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.BaseFeatures;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;

/// <summary>
///     An abstract that hooks OnSetup to provide a feature.
/// </summary>
internal abstract class OnSetupFeature : IBaseFeature
{
#pragma warning disable SA1401
    protected readonly YesModule Module;
#pragma warning restore SA1401
    private readonly AddonEvent trigger;

    private bool enabled;

    /// <summary>
    ///     Initializes a new instance of the <see cref="OnSetupFeature"/> class.
    /// </summary>
    /// <param name="module">The module.</param>
    /// <param name="trigger">The event that triggers the feature.</param>
    protected OnSetupFeature(YesModule module, AddonEvent trigger = AddonEvent.PostRequestedUpdate)
    {
        this.Module = module;
        this.trigger = trigger;
    }

    /// <summary>
    ///     Gets the <see cref="YesConfiguration"/>.
    /// </summary>
    protected YesConfiguration Configuration => this.Module.MC;

    /// <summary>
    ///     Gets the name of the addon being hooked.
    /// </summary>
    protected abstract string AddonName { get; }

    /// <summary>
    ///     Gets a value indicating whether the feature is enable in the configuration.
    /// </summary>
    protected abstract bool ConfigurationEnableState { get; }

    /// <inheritdoc/>
    public void Dispose() => this.OnDisable();

    public void UpdateEnableState()
    {
        if (this.ConfigurationEnableState)
        {
            this.OnEnable();
        }
        else
        {
            this.OnDisable();
        }
    }

    public virtual bool OnEnable()
    {
        if (this.enabled)
        {
            // is already enabled.
            return false;
        }

        this.enabled = true;

        this.Module.DalamudServices.AddonLifecycle.RegisterListener(this.trigger, this.AddonName, this.TriggerHandler);
        return true;
    }

    public virtual bool OnDisable()
    {
        if (!this.enabled)
        {
            // can't disable a non enabled state.
            return false;
        }

        this.enabled = false;

        this.Module.DalamudServices.AddonLifecycle.UnregisterListener(this.trigger, this.AddonName, this.TriggerHandler);
        return true;
    }

    /// <summary>
    ///     A method that is run within the OnSetup detour.
    /// </summary>
    /// <param name="addon">Addon address.</param>
    /// <param name="addonEvent">Addon trigger event.</param>
    protected abstract void OnSetupImpl(IntPtr addon, AddonEvent addonEvent);

    private void TriggerHandler(AddonEvent type, AddonArgs args)
    {
        // if (this.trigger is not (AddonEvent.PostUpdate or AddonEvent.PostDraw))
        {
            this.Module.Logger.Debug($"Addon{this.AddonName}.OnSetup");
        }

        if (!this.Module.MC.ModuleEnabled || this.Module.DisableKeyPressed)
        {
            return;
        }

        if (!this.ConfigurationEnableState)
        {
            return;
        }

        try
        {
            this.OnSetupImpl(args.Addon, type);
        }
        catch (Exception ex)
        {
            this.Module.Logger.Error(ex, "Don't crash the game");
        }
    }
}