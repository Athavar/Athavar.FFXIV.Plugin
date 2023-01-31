// <copyright file="AddonMaterializeDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;

/// <summary>
///     AddonMaterializeDialog feature.
/// </summary>
internal class AddonMaterializeDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonMaterializeDialogFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    /// <param name="services">ServiceContainer of all dalamud services.</param>
    public AddonMaterializeDialogFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 48 89 7C 24 ?? 41 56 48 83 EC 50 44 8B F2 49 8B E8 BA ?? ?? ?? ??", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "MaterializeDialog";

    /// <inheritdoc />
    protected override void OnSetupImpl(nint addon, uint a2, nint data)
    {
        if (!this.Configuration.MaterializeDialogEnabled)
        {
            return;
        }

        ClickMaterializeDialog.Using(addon).Materialize();
    }
}