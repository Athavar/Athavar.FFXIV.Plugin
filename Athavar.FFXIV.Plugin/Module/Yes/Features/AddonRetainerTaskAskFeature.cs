// <copyright file="AddonRetainerTaskAskFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;

    /// <summary>
    /// AddonRetainerTaskAsk feature.
    /// </summary>
    internal class AddonRetainerTaskAskFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonRetainerTaskAskFeature"/> class.
        /// </summary>
        public AddonRetainerTaskAskFeature()
            : base(YesService.Address.AddonRetainerTaskAskOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "RetainerTaskAsk";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.RetainerTaskAskEnabled)
            {
                return;
            }

            ClickRetainerTaskAsk.Using(addon).Assign();
        }
    }
}
