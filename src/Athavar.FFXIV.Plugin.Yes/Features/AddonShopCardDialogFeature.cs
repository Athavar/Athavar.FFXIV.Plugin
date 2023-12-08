// <copyright file="AddonShopCardDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonShopCardDialog feature.
/// </summary>
internal class AddonShopCardDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonShopCardDialogFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    /// <param name="services">ServiceContainer of all dalamud services.</param>
    public AddonShopCardDialogFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "ShopCardDialog";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.ShopCardDialog;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AddonShopCardDialog*)addon;
        if (addonPtr->CardQuantityInput != null)
        {
            addonPtr->CardQuantityInput->SetValue(addonPtr->CardQuantityInput->Data.Max);
        }

        ClickShopCardDialog.Using(addon).Sell();
    }
}