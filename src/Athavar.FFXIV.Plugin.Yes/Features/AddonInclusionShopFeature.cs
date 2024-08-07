// <copyright file="AddonInclusionShopFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Structures;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     AddonInclusionShop feature.
///     Disabled.
/// </summary>
internal abstract class AddonInclusionShopFeature : OnSetupFeature, IDisposable
{
    private Hook<AgentReceiveEventDelegate>? agentReceiveEventHook;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonInclusionShopFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonInclusionShopFeature(YesModule module)
        : base(module, AddonEvent.PostSetup)
    {
    }

    private unsafe delegate nint AgentReceiveEventDelegate(nint agent, nint eventData, AtkValue* values, uint valueCount, ulong eventKind);

    /// <inheritdoc/>
    protected override string AddonName => "InclusionShop";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.InclusionShopRememberEnabled;

    /// <inheritdoc/>
    public override unsafe bool OnEnable()
    {
        if (!base.OnEnable())
        {
            return false;
        }

        if (this.Module.AddressResolver.AgentReceiveEvent == nint.Zero)
        {
            return false;
        }

        this.Module.DalamudServices.SafeEnableHookFromAddress<AgentReceiveEventDelegate>("AddonInclusionShopFeature:agentReceiveEventHook", this.Module.AddressResolver.AgentReceiveEvent, this.AgentReceiveEventDetour, h => this.agentReceiveEventHook = h);
        return true;
    }

    /// <inheritdoc/>
    public override bool OnDisable()
    {
        if (!base.OnDisable())
        {
            return false;
        }

        this.agentReceiveEventHook?.Disable();
        this.agentReceiveEventHook?.Dispose();
        this.agentReceiveEventHook = null;
        return true;
    }

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var unitbase = (AtkUnitBase*)addon;

        this.Module.Logger.Debug($"Firing 12,{this.Configuration.InclusionShopRememberCategory}");
        using var categoryValues = new AtkValueArray(12, this.Configuration.InclusionShopRememberCategory);
        unitbase->FireCallback(2, categoryValues);

        this.Module.Logger.Debug($"Firing 13,{this.Configuration.InclusionShopRememberSubcategory}");
        using var subcategoryValues = new AtkValueArray(13, this.Configuration.InclusionShopRememberSubcategory);
        unitbase->FireCallback(2, subcategoryValues);
    }

    private unsafe nint AgentReceiveEventDetour(nint agent, nint eventData, AtkValue* values, uint valueCount, ulong eventKind)
    {
        var result = this.agentReceiveEventHook?.OriginalDisposeSafe(agent, eventData, values, valueCount, eventKind) ?? nint.Zero;

        if (valueCount != 2)
        {
            return result;
        }

        var atkValue0 = values[0];
        if (atkValue0.Type != ValueType.Int)
        {
            return result;
        }

        var val0 = atkValue0.Int;
        if (val0 == 12)
        {
            var val1 = values[1].UInt;
            if (val1 != this.Configuration.InclusionShopRememberCategory)
            {
                this.Module.Logger.Debug($"Remembering InclusionShop category: {val1}");
                this.Configuration.InclusionShopRememberCategory = val1;
                this.Configuration.InclusionShopRememberSubcategory = 0;
                this.Configuration.Save();
            }
        }
        else if (val0 == 13)
        {
            var val1 = values[1].UInt;
            if (val1 != this.Configuration.InclusionShopRememberSubcategory)
            {
                this.Module.Logger.Debug($"Remembering InclusionShop subcategory: {val1}");
                this.Configuration.InclusionShopRememberSubcategory = val1;
                this.Configuration.Save();
            }
        }

        return result;
    }
}