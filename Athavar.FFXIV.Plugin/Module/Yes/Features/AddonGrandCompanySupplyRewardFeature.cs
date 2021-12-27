// <copyright file="AddonGrandCompanySupplyRewardFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

/// <summary>
///     AddonGrandCompanySupplyReward feature.
/// </summary>
internal class AddonGrandCompanySupplyRewardFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonGrandCompanySupplyRewardFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonGrandCompanySupplyRewardFeature(YesModule module)
        : base(module.AddressResolver.AddonGrandCompanySupplyRewardOnSetupAddress, module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "GrandCompanySupplyReward";

    /// <inheritdoc />
    protected override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.GrandCompanySupplyReward)
        {
            return;
        }

        ClickGrandCompanySupplyReward.Using(addon).Deliver();
    }
}