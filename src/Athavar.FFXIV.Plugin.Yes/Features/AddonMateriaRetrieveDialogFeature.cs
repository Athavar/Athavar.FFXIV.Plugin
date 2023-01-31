// <copyright file="AddonMateriaRetrieveDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;

/// <summary>
///     AddonMateriaRetrieveDialog feature.
/// </summary>
internal class AddonMateriaRetrieveDialogFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonMateriaRetrieveDialogFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    /// <param name="services">ServiceContainer of all dalamud services.</param>
    public AddonMateriaRetrieveDialogFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B FA 49 8B D8 BA ?? ?? ?? ?? 48 8B F1 E8 ?? ?? ?? ?? 48 8B C8", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "MateriaRetrieveDialog";

    /// <inheritdoc />
    protected override void OnSetupImpl(nint addon, uint a2, nint data)
    {
        if (!this.Configuration.MateriaRetrieveDialogEnabled)
        {
            return;
        }

        ClickMateriaRetrieveDialog.Using(addon).Begin();
    }
}