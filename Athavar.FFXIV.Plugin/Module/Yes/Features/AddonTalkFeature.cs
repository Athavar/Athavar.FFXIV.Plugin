// <copyright file="AddonTalkFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using System.Linq;
    using ClickLib.Clicks;
    using Dalamud.Logging;
    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// AddonTalk feature.
    /// </summary>
    internal class AddonTalkFeature : UpdateFeature
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AddonTalkFeature"/> class.
        /// </summary>
        public AddonTalkFeature()
            : base(YesService.Address.AddonTalkUpdateAddress)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "Talk";

        /// <inheritdoc/>
        protected unsafe override void UpdateImpl(IntPtr addon, IntPtr a2, IntPtr a3)
        {
            var addonPtr = (AddonTalk*)addon;
            if (!addonPtr->AtkUnitBase.IsVisible)
            {
                return;
            }

            var target = DalamudBinding.TargetManager.Target;
            var targetName = YesService.Module!.LastSeenTalkTarget = target != null
                ? YesModule.GetSeStringText(target.Name)
                : string.Empty;

            var nodes = YesService.Configuration.GetAllNodes().OfType<TalkEntryNode>();
            foreach (var node in nodes)
            {
                if (!node.Enabled || string.IsNullOrEmpty(node.TargetText))
                {
                    continue;
                }

                var matched = this.EntryMatchesTargetName(node, targetName);
                if (!matched)
                {
                    continue;
                }

                PluginLog.Debug("AddonTalk: Advancing");
                ClickTalk.Using(addon).Click();
                return;
            }
        }

        private unsafe void AddonSelectYesNoExecute(IntPtr addon, bool yes)
        {
            if (yes)
            {
                var addonPtr = (AddonSelectYesno*)addon;
                var yesButton = addonPtr->YesButton;
                if (yesButton != null && !yesButton->IsEnabled)
                {
                    PluginLog.Debug("AddonSelectYesNo: Enabling yes button");
                    yesButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
                }

                PluginLog.Debug("AddonSelectYesNo: Selecting yes");
                ClickSelectYesNo.Using(addon).Yes();
            }
            else
            {
                PluginLog.Debug("AddonSelectYesNo: Selecting no");
                ClickSelectYesNo.Using(addon).No();
            }
        }

        private bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        {
            return (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
                  (!node.TargetIsRegex && targetName.Contains(node.TargetText));
        }
    }
}
