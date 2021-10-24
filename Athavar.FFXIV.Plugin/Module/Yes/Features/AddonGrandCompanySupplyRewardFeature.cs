// <copyright file="AddonGrandCompanySupplyRewardFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;

    /// <summary>
    /// AddonGrandCompanySupplyReward feature.
    /// </summary>
    internal class AddonGrandCompanySupplyRewardFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonGrandCompanySupplyRewardFeature"/> class.
        /// </summary>
        public AddonGrandCompanySupplyRewardFeature()
            : base(YesService.Address.AddonGrandCompanySupplyRewardOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "GrandCompanySupplyReward";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.GrandCompanySupplyReward)
            {
                return;
            }

            ClickGrandCompanySupplyReward.Using(addon).Deliver();
        }
    }
}
