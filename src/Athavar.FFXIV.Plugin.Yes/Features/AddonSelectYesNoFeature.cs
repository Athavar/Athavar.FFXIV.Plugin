﻿// <copyright file="AddonSelectYesNoFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     AddonSelectYesNo feature.
/// </summary>
internal class AddonSelectYesNoFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSelectYesNoFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonSelectYesNoFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SelectYesno";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.FunctionEnabled;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AddonSelectYesno*)addon;
        if (addonPtr == null)
        {
            return;
        }

        var text = this.Module.LastSeenDialogText = this.Module.GetSeStringText(addonPtr->PromptText->NodeText.StringPtr);
        this.Module.Logger.Debug($"AddonSelectYesNo: text={text}");

        if (this.Module.ForcedYesKeyPressed)
        {
            this.Module.Logger.Debug("AddonSelectYesNo: Forced yes hotkey pressed");
            this.AddonSelectYesNoExecute(addon, true);
            return;
        }

        var zoneWarnOnce = true;
        var nodes = this.Configuration.GetAllNodes().OfType<TextEntryNode>();
        foreach (var node in nodes)
        {
            if (!node.Enabled || string.IsNullOrEmpty(node.Text))
            {
                continue;
            }

            if (!this.EntryMatchesText(node, text))
            {
                continue;
            }

            if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
            {
                if (!this.Module.TerritoryNames.TryGetValue(this.Module.DalamudServices.ClientState.TerritoryType, out var zoneName))
                {
                    if (zoneWarnOnce && !(zoneWarnOnce = false))
                    {
                        this.Module.Logger.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                        this.Module.ChatManager.PrintChat("Unable to verify Zone Restricted entry, change zones to update value");
                    }

                    zoneName = string.Empty;
                }

                if (!string.IsNullOrEmpty(zoneName) && this.EntryMatchesZoneName(node, zoneName))
                {
                    this.Module.Logger.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                    this.AddonSelectYesNoExecute(addon, node.IsYes);
                    return;
                }
            }
            else
            {
                this.Module.Logger.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                this.AddonSelectYesNoExecute(addon, node.IsYes);
                return;
            }
        }
    }

    private unsafe void AddonSelectYesNoExecute(nint addon, bool yes)
    {
        if (yes)
        {
            var addonPtr = (AddonSelectYesno*)addon;
            var yesButton = addonPtr->YesButton;
            if (yesButton != null && !yesButton->IsEnabled)
            {
                this.Module.Logger.Debug("AddonSelectYesNo: Enabling yes button");
                yesButton->AtkComponentBase.OwnerNode->AtkResNode.NodeFlags |= NodeFlags.Enabled;
            }

            this.Module.Logger.Debug("AddonSelectYesNo: Selecting yes");
            ClickSelectYesNo.Using(addon).Yes();
        }
        else
        {
            this.Module.Logger.Debug("AddonSelectYesNo: Selecting no");
            ClickSelectYesNo.Using(addon).No();
        }
    }

    private bool EntryMatchesText(TextEntryNode node, string text)
        => (node.IsTextRegex && (node.TextRegex.Value?.IsMatch(text) ?? false)) ||
           (!node.IsTextRegex && text.Contains(node.Text));

    private bool EntryMatchesZoneName(TextEntryNode node, string zoneName)
        => (node.ZoneIsRegex && (node.ZoneRegex.Value?.IsMatch(zoneName) ?? false)) ||
           (!node.ZoneIsRegex && zoneName.Contains(node.ZoneText));

    [StructLayout(LayoutKind.Explicit, Size = 0x10)]
    private struct AddonSelectYesNoOnSetupData
    {
        [FieldOffset(0x8)]
        public readonly nint TextPtr;
    }
}