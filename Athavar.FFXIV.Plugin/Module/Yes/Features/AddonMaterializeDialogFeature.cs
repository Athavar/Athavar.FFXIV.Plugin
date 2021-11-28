// <copyright file="AddonMaterializeDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

/// <summary>
///     AddonMaterializeDialog feature.
/// </summary>
internal class AddonMaterializeDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonMaterializeDialogFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonMaterializeDialogFeature(YesModule module)
        : base(module.AddressResolver.AddonMaterializeDialongOnSetupAddress, module.Configuration)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "MaterializeDialog";

    /// <inheritdoc />
    protected override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.MaterializeDialogEnabled)
        {
            return;
        }

        ClickMaterializeDialog.Using(addon).Materialize();
    }
}