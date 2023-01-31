// <copyright file="AddonRetainerTaskResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonRetainerTaskResult feature.
/// </summary>
internal class AddonRetainerTaskResultFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonRetainerTaskResultFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonRetainerTaskResultFeature(YesModule module)
        : base("48 89 5C 24 ?? 55 56 57 48 83 EC 40 8B F2 49 8B F8 BA ?? ?? ?? ?? 48 8B D9 E8 ?? ?? ?? ??", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "RetainerTaskResult";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(nint addon, uint a2, nint data)
    {
        if (!this.Configuration.RetainerTaskResultEnabled)
        {
            return;
        }

        var addonPtr = (AddonRetainerTaskResult*)addon;
        var buttonText = addonPtr->ReassignButton->ButtonTextNode->NodeText.ToString();
        if (buttonText is "Recall" or "中断する" or "Zurückrufen" or "Interrompre")
        {
            return;
        }

        ClickRetainerTaskResult.Using(addon).Reassign();
    }
}