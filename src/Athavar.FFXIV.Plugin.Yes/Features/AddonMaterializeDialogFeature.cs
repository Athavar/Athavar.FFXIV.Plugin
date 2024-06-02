// <copyright file="AddonMaterializeDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;

/// <summary>
///     AddonMaterializeDialog feature.
/// </summary>
internal class AddonMaterializeDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonMaterializeDialogFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonMaterializeDialogFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "MaterializeDialog";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.MaterializeDialogEnabled;

    /// <inheritdoc/>
    protected override void OnSetupImpl(IntPtr addon, AddonEvent addonEvent) => ClickMaterializeDialog.Using(addon).Materialize();
}