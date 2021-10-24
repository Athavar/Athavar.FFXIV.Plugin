// <copyright file="AddonSalvageDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;
    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// AddonSalvageDialog feature.
    /// </summary>
    internal class AddonSalvageDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonSalvageDialogFeature"/> class.
        /// </summary>
        public AddonSalvageDialogFeature()
            : base(YesService.Address.AddonSalvageDialongOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "SalvageDialog";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (YesService.Configuration.DesynthBulkDialogEnabled)
            {
                ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
            }

            if (YesService.Configuration.DesynthDialogEnabled)
            {
                var clickAddon = ClickSalvageDialog.Using(addon);
                clickAddon.CheckBox();
                clickAddon.Desynthesize();
            }
        }
    }
}
