// <copyright file="AddonContentsFinderConfirmFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;

    /// <summary>
    /// AddonContentsFinderConfirm feature.
    /// </summary>
    internal class AddonContentsFinderConfirmFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonContentsFinderConfirmFeature"/> class.
        /// </summary>
        public AddonContentsFinderConfirmFeature()
            : base(YesService.Address.AddonContentsFinderConfirmOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "ContentsFinderConfirm";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.ContentsFinderConfirmEnabled)
            {
                return;
            }

            ClickContentsFinderConfirm.Using(addon).Commence();
        }
    }
}
