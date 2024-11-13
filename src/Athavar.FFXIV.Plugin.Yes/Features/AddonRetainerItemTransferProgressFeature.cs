// <copyright file="AddonRetainerItemTransferProgressFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

internal class AddonRetainerItemTransferProgressFeature : OnSetupFeature
{
    private readonly string retainerEntrustItemsSuccessfulText;

    public AddonRetainerItemTransferProgressFeature(YesModule module)
        : base(module, AddonEvent.PostUpdate)
        => this.retainerEntrustItemsSuccessfulText = this.Module.DalamudServices.DataManager.GetExcelSheet<Addon>()?.GetRowOrDefault(13528)?.Text.ExtractText() ?? throw new AthavarPluginException("Sheet Addon or row 13528 not found");

    /// <inheritdoc/>
    protected override string AddonName => "RetainerItemTransferProgress";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.RetainerTransferProgressConfirmEnable;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AtkUnitBase*)addon;

        // check if entrust items was successful.
        if (this.Module.GetSeStringText(addonPtr->AtkValues[0].String) == this.retainerEntrustItemsSuccessfulText)
        {
            this.Module.Logger.Debug("Closing Entrust Duplicates menu");
            addonPtr->Close(true);
        }
    }
}