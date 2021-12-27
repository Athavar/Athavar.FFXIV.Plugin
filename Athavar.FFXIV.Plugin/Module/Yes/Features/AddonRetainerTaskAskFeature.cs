// <copyright file="AddonRetainerTaskAskFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

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
        : base(module.AddressResolver.AddonRetainerTaskAskOnSetupAddress, module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "RetainerTaskAsk";

    /// <inheritdoc />
    protected override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.RetainerTaskAskEnabled)
        {
            return;
        }

        ClickRetainerTaskAsk.Using(addon).Assign();
    }
}