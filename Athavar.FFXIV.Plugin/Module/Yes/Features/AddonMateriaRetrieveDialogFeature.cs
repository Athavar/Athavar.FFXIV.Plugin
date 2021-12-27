// <copyright file="AddonMateriaRetrieveDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;

/// <summary>
///     AddonMateriaRetrieveDialog feature.
/// </summary>
internal class AddonMateriaRetrieveDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonMateriaRetrieveDialogFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonMateriaRetrieveDialogFeature(YesModule module)
        : base(module.AddressResolver.AddonMateriaRetrieveDialogOnSetupAddress, module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "MateriaRetrieveDialog";

    /// <inheritdoc />
    protected override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.MateriaRetrieveDialogEnabled)
        {
            return;
        }

        ClickMateriaRetrieveDialog.Using(addon).Begin();
    }
}