// <copyright file="AddonPurifyResultFeature.cs" company="Athavar">
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

/// <summary>
///     AddonMateriaRetrieveDialog feature.
/// </summary>
internal class AddonPurifyResultFeature : OnSetupFeature
{
    private readonly string aetherialReductionSuccessfulText;

    public AddonPurifyResultFeature(YesModule module)
        : base(module)
        => this.aetherialReductionSuccessfulText = this.module.DalamudServices.DataManager.GetExcelSheet<Addon>()?.GetRow(2171)?.Text.RawString ?? throw new AthavarPluginException("Sheet Addon or row 2171 not found");

    protected override string AddonName => "PurifyResult";

    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AtkUnitBase*)addon;

        if (!this.Configuration.AetherialReductionPurifyResultEnabled)
        {
            return;
        }

        // close only if successful.
        if (addonPtr->UldManager.NodeList[17]->GetAsAtkTextNode()->NodeText.ToString() == this.aetherialReductionSuccessfulText)
        {
            this.module.Logger.Debug("Closing Purify Results menu");
            ClickPurifyResult.Using(addon).Close();
        }
    }
}