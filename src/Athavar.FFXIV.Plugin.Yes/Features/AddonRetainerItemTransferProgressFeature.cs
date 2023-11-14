// <copyright file="AddonRetainerItemTransferProgressFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;

internal class AddonRetainerItemTransferProgressFeature : OnSetupFeature
{
    private readonly string retainerEntrustItemsSuccessfulText;

    public AddonRetainerItemTransferProgressFeature(YesModule module)
        : base(module, AddonEvent.PostUpdate)
        => this.retainerEntrustItemsSuccessfulText = this.module.DalamudServices.DataManager.GetExcelSheet<Addon>()?.GetRow(13528)?.Text.RawString ?? throw new AthavarPluginException("Sheet Addon or row 13528 not found");

    protected override string AddonName => "RetainerItemTransferProgress";

    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AtkUnitBase*)addon;

        if (!this.Configuration.RetainerTransferProgressConfirmEnable)
        {
            return;
        }

        // check if entrust items was successful.
        if (this.module.GetSeStringText(addonPtr->AtkValues[0].String) == this.retainerEntrustItemsSuccessfulText)
        {
            this.module.Logger.Debug("Closing Entrust Duplicates menu");

            // simple use "ClickPurifyResult" for triggering the closing of the addon.
            ClickPurifyResult.Using(addon).Close();
        }
    }
}