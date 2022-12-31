// <copyright file="AddonInclusionShopFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Structures;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;
using Dalamud.Hooking;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ValueType = FFXIVClientStructs.FFXIV.Component.GUI.ValueType;

/// <summary>
///     AddonInclusionShop feature.
/// </summary>
internal class AddonInclusionShopFeature : OnSetupFeature, IDisposable
{
    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B DA 4D 8B D0 32 D2", DetourName = nameof(AgentReceiveEventDetour))]
    private readonly Hook<AgentReceiveEventDelegate> agentReceiveEventHook = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonInclusionShopFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonInclusionShopFeature(YesModule module)
        : base("85 D2 0F 8E ?? ?? ?? ?? 4C 8B DC 55 53 41 54", module)
    {
        SignatureHelper.Initialise(this);

        this.agentReceiveEventHook.Enable();
    }

    private unsafe delegate IntPtr AgentReceiveEventDelegate(IntPtr agent, IntPtr eventData, AtkValue* values, uint valueCount, ulong eventKind);

    /// <inheritdoc />
    protected override string AddonName => "InclusionShop";

    /// <inheritdoc />
    public new void Dispose()
    {
        this.agentReceiveEventHook.Disable();
        this.agentReceiveEventHook.Dispose();
        base.Dispose();
    }

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.InclusionShopRememberEnabled)
        {
            return;
        }

        var unitbase = (AtkUnitBase*)addon;

        PluginLog.Debug($"Firing 12,{this.Configuration.InclusionShopRememberCategory}");
        using var categoryValues = new AtkValueArray(12, this.Configuration.InclusionShopRememberCategory);
        unitbase->FireCallback(2, categoryValues);

        PluginLog.Debug($"Firing 13,{this.Configuration.InclusionShopRememberSubcategory}");
        using var subcategoryValues = new AtkValueArray(13, this.Configuration.InclusionShopRememberSubcategory);
        unitbase->FireCallback(2, subcategoryValues);
    }

    private unsafe IntPtr AgentReceiveEventDetour(IntPtr agent, IntPtr eventData, AtkValue* values, uint valueCount, ulong eventKind)
    {
        IntPtr Original() => this.agentReceiveEventHook.Original(agent, eventData, values, valueCount, eventKind);

        if (valueCount != 2)
        {
            return Original();
        }

        var atkValue0 = values[0];
        if (atkValue0.Type != ValueType.Int)
        {
            return Original();
        }

        var val0 = atkValue0.Int;
        if (val0 == 12)
        {
            var val1 = values[1].UInt;
            if (val1 != this.Configuration.InclusionShopRememberCategory)
            {
                PluginLog.Debug($"Remembring InclusionShop category: {val1}");
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
                PluginLog.Debug($"Remembring InclusionShop subcategory: {val1}");
                this.Configuration.InclusionShopRememberSubcategory = val1;
                this.Configuration.Save();
            }
        }

        return Original();
    }
}