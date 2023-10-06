// <copyright file="AddonMateriaRetrieveDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;

/// <summary>
///     AddonMateriaRetrieveDialog feature.
/// </summary>
internal class AddonMateriaRetrieveDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonMateriaRetrieveDialogFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonMateriaRetrieveDialogFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "MateriaRetrieveDialog";

    /// <inheritdoc/>
    protected override void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        if (!this.Configuration.MateriaRetrieveDialogEnabled)
        {
            return;
        }

        ClickMateriaRetrieveDialog.Using(addon).Begin();
    }
}