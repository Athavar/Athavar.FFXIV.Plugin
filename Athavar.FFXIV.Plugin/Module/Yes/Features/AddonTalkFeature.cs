// <copyright file="AddonTalkFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using System.Linq;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonTalk feature.
/// </summary>
internal class AddonTalkFeature : UpdateFeature
{
    private readonly YesModule module;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonTalkFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonTalkFeature(YesModule module)
        : base(module.AddressResolver.AddonTalkUpdateAddress, module) =>
        this.module = module;

    /// <inheritdoc />
    protected override string AddonName => "Talk";

    /// <inheritdoc />
    protected override unsafe void UpdateImpl(IntPtr addon, IntPtr a2, IntPtr a3)
    {
        var addonPtr = (AddonTalk*)addon;
        if (!addonPtr->AtkUnitBase.IsVisible)
        {
            return;
        }

        var target = this.module.DalamudServices.TargetManager.Target;
        var targetName = this.module.LastSeenTalkTarget = target != null
            ? this.module.GetSeStringText(target.Name)
            : string.Empty;

        var nodes = this.Configuration.GetAllNodes().OfType<TalkEntryNode>();
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

    private bool EntryMatchesTargetName(TalkEntryNode node, string targetName) =>
        (node.TargetIsRegex && (node.TargetRegex?.IsMatch(targetName) ?? false)) ||
        (!node.TargetIsRegex && targetName.Contains(node.TargetText));
}