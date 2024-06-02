// <copyright file="AddonRetainerTaskResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonRetainerTaskResult feature.
/// </summary>
internal class AddonRetainerTaskResultFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonRetainerTaskResultFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonRetainerTaskResultFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "RetainerTaskResult";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.RetainerTaskResultEnabled;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AddonRetainerTaskResult*)addon;
        var buttonText = addonPtr->ReassignButton->ButtonTextNode->NodeText.ToString();
        if (buttonText is "Recall" or "中断する" or "Zurückrufen" or "Interrompre")
        {
            return;
        }

        ClickRetainerTaskResult.Using(addon).Reassign();
    }
}