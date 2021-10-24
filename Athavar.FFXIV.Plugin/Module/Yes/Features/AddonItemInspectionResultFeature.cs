// <copyright file="AddonItemInspectionResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using ClickLib.Clicks;
    using Dalamud.Game.Text.SeStringHandling.Payloads;
    using FFXIVClientStructs.FFXIV.Client.UI;
    using FFXIVClientStructs.FFXIV.Component.GUI;

    /// <summary>
    /// AddonItemInspectionResult feature.
    /// </summary>
    internal class AddonItemInspectionResultFeature : OnSetupFeature
    {
        private int itemInspectionCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddonItemInspectionResultFeature"/> class.
        /// </summary>
        public AddonItemInspectionResultFeature()
            : base(YesService.Address.AddonItemInspectionResultOnSetupAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "ItemInspectionResult";

        /// <inheritdoc/>
        protected unsafe override void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
        {
            if (!YesService.Configuration.ItemInspectionResultEnabled)
            {
                return;
            }

            var addonPtr = (AddonItemInspectionResult*)addon;
            if (addonPtr->AtkUnitBase.UldManager.NodeListCount < 64)
            {
                return;
            }

            var nameNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[64];
            var descNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[55];
            if (!nameNode->AtkResNode.IsVisible || !descNode->AtkResNode.IsVisible)
            {
                return;
            }

            var nameText = YesModule.GetSeString(nameNode->NodeText.StringPtr);
            var descText = YesModule.GetSeStringText(descNode->NodeText.StringPtr);

            // This is hackish, but works well enough (for now).
            // Languages that dont contain the magic character will need special handling.
            if (descText.Contains("※") || descText.Contains("liées à Garde-la-Reine"))
            {
                nameText.Payloads.Insert(0, new TextPayload("Received: "));
                YesService.Module!.PrintMessage(nameText);
            }

            this.itemInspectionCount++;
            var rateLimiter = YesService.Configuration.ItemInspectionResultRateLimiter;
            if (rateLimiter != 0 && this.itemInspectionCount % rateLimiter == 0)
            {
                this.itemInspectionCount = 0;
                YesService.Module!.PrintMessage("Rate limited, pausing item inspection loop.");
                return;
            }

            ClickItemInspectionResult.Using(addon).Next();
        }
    }
}
