// <copyright file="AddonRetainerTaskAskFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;

/// <summary>
///     AddonRetainerTaskAsk feature.
/// </summary>
internal class AddonRetainerTaskAskFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonRetainerTaskAskFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonRetainerTaskAskFeature(YesModule module)
        : base("40 53 48 83 EC 30 48 8B D9 83 FA 03 7C 53 49 8B C8 E8 ?? ?? ?? ??", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "RetainerTaskAsk";

    /// <inheritdoc />
    protected override void OnSetupImpl(nint addon, uint a2, nint data)
    {
        if (!this.Configuration.RetainerTaskAskEnabled)
        {
            return;
        }

        ClickRetainerTaskAsk.Using(addon).Assign();
    }
}