// <copyright file="AddonPurifyResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

/// <summary>
///     AddonMateriaRetrieveDialog feature.
/// </summary>
internal class AddonPurifyResultFeature : OnSetupFeature
{
    private readonly string aetherialReductionSuccessfulText;

    public AddonPurifyResultFeature(YesModule module)
        : base(module)
        => this.aetherialReductionSuccessfulText = this.Module.DalamudServices.DataManager.GetExcelSheet<Addon>()?.GetRowOrDefault(2171)?.Text.ExtractText() ?? throw new AthavarPluginException("Sheet Addon or row 2171 not found");

    /// <inheritdoc/>
    protected override string AddonName => "PurifyResult";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.AetherialReductionPurifyResultEnabled;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var unitBase = (AtkUnitBase*)addon;

        // close only if successful.
        if (unitBase->UldManager.NodeList[17]->GetAsAtkTextNode()->NodeText.ToString() == this.aetherialReductionSuccessfulText)
        {
            this.Module.Logger.Debug("Closing Purify Results menu");
            unitBase->Close(true);
        }
    }
}