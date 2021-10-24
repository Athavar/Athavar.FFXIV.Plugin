// <copyright file="AddonShopCardDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;
    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// AddonShopCardDialog feature.
    /// </summary>
    internal class AddonShopCardDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonShopCardDialogFeature"/> class.
        /// </summary>
        public AddonShopCardDialogFeature()
            : base(YesService.Address.AddonShopCardDialogOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "ShopCardDialog";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.ShopCardDialog)
            {
                return;
            }

            var addonPtr = (AddonShopCardDialog*)addon;
            if (addonPtr->CardQuantityInput != null)
            {
                addonPtr->CardQuantityInput->SetValue(addonPtr->CardQuantityInput->Data.Max);
            }

            ClickShopCardDialog.Using(addon).Sell();
        }
    }
}
