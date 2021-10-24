// <copyright file="AddonMateriaRetrieveDialogFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;

    /// <summary>
    /// AddonMateriaRetrieveDialog feature.
    /// </summary>
    internal class AddonMateriaRetrieveDialogFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonMateriaRetrieveDialogFeature"/> class.
        /// </summary>
        public AddonMateriaRetrieveDialogFeature()
            : base(YesService.Address.AddonMateriaRetrieveDialongOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "MateriaRetrieveDialog";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.MateriaRetrieveDialogEnabled)
            {
                return;
            }

            ClickMateriaRetrieveDialog.Using(addon).Begin();
        }
    }
}
