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
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 83 EC 30 BA ?? ?? ?? ?? 4D 8B E8 4C 8B F9", module)
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