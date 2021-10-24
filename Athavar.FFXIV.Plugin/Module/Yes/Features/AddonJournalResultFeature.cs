// <copyright file="AddonJournalResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;
    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// AddonJournalResult feature.
    /// </summary>
    internal class AddonJournalResultFeature : OnSetupFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonJournalResultFeature"/> class.
        /// </summary>
        public AddonJournalResultFeature()
            : base(YesService.Address.AddonJournalResultOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "JournalResult";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.JournalResultCompleteEnabled)
            {
                return;
            }

            var addonPtr = (AddonJournalResult*)addon;
            var completeButton = addonPtr->CompleteButton;
            if (!addonPtr->CompleteButton->IsEnabled)
            {
                return;
            }

            ClickJournalResult.Using(addon).Complete();
        }
    }
}
