// <copyright file="AddonRetainerTaskResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;

    /// <summary>
    /// AddonRetainerTaskResult feature.
    /// </summary>
    internal class AddonRetainerTaskResultFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonRetainerTaskResultFeature"/> class.
        /// </summary>
        public AddonRetainerTaskResultFeature()
            : base(YesService.Address.AddonRetainerTaskResultOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "RetainerTaskResult";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.RetainerTaskResultEnabled)
            {
                return;
            }

            ClickRetainerTaskResult.Using(addon).Reassign();
        }
    }
}
