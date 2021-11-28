// <copyright file="AddonShopCardDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonShopCardDialog feature.
/// </summary>
internal class AddonShopCardDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonShopCardDialogFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonShopCardDialogFeature(YesModule module)
        : base(module.AddressResolver.AddonShopCardDialogOnSetupAddress, module.Configuration)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "ShopCardDialog";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.ShopCardDialog)
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