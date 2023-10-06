// <copyright file="AddonSalvageDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonSalvageDialog feature.
/// </summary>
internal class AddonSalvageDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSalvageDialogFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonSalvageDialogFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SalvageDialog";

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        if (this.Configuration.DesynthBulkDialogEnabled)
        {
            ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
        }

        if (this.Configuration.DesynthDialogEnabled)
        {
            var clickAddon = ClickSalvageDialog.Using(addon);
            clickAddon.CheckBox();
            clickAddon.Desynthesize();
        }
    }
}