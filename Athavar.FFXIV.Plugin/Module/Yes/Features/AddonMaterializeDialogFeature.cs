// <copyright file="AddonMaterializeDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;

    /// <summary>
    /// AddonMaterializeDialog feature.
    /// </summary>
    internal class AddonMaterializeDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonMaterializeDialogFeature"/> class.
        /// </summary>
        public AddonMaterializeDialogFeature()
            : base(YesService.Address.AddonMaterializeDialongOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "MaterializeDialog";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.MaterializeDialogEnabled)
            {
                return;
            }

            ClickMaterializeDialog.Using(addon).Materialize();
        }
    }
}
