// <copyright file="AddonSalvageDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonSalvageDialog feature.
/// </summary>
internal class AddonSalvageDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSalvageDialogFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonSalvageDialogFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 30 44 8B F2 49 8B E8", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "SalvageDialog";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(nint addon, uint a2, nint data)
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