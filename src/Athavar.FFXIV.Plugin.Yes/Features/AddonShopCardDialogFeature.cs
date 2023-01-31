// <copyright file="AddonShopCardDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
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
    /// <param name="services">ServiceContainer of all dalamud services.</param>
    public AddonShopCardDialogFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 54 41 56 41 57 48 83 EC 50 48 8B F9 49 8B F0", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "ShopCardDialog";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(nint addon, uint a2, nint data)
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