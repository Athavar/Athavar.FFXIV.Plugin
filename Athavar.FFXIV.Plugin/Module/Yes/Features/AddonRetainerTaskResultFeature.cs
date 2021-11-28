// <copyright file="AddonRetainerTaskResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

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
        : base(module.AddressResolver.AddonRetainerTaskResultOnSetupAddress, module.Configuration)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "RetainerTaskResult";

    /// <inheritdoc />
    protected override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.RetainerTaskResultEnabled)
        {
            return;
        }

        ClickRetainerTaskResult.Using(addon).Reassign();
    }
}