// <copyright file="AddonItemInspectionResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes.Features;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     AddonItemInspectionResult feature.
/// </summary>
internal class AddonItemInspectionResultFeature : OnSetupFeature
{
    private readonly YesModule module;
    private int itemInspectionCount;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonItemInspectionResultFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonItemInspectionResultFeature(YesModule module)
        : base(module.AddressResolver.AddonItemInspectionResultOnSetupAddress, module) =>
        this.module = module;

    /// <inheritdoc />
    protected override string AddonName => "ItemInspectionResult";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(IntPtr addon, uint a2, IntPtr data)
    {
        if (!this.Configuration.ItemInspectionResultEnabled)
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

        var nameText = this.module.GetSeString(nameNode->NodeText.StringPtr);
        var descText = this.module.GetSeStringText(descNode->NodeText.StringPtr);

        // This is hackish, but works well enough (for now).
        // Languages that dont contain the magic character will need special handling.
        if (descText.Contains("※") || descText.Contains("liées à Garde-la-Reine"))
        {
            nameText.Payloads.Insert(0, new TextPayload("Received: "));
            this.module.ChatManager.PrintInformationMessage(nameText);
        }

        this.itemInspectionCount++;
        var rateLimiter = this.Configuration.ItemInspectionResultRateLimiter;
        if (rateLimiter != 0 && this.itemInspectionCount % rateLimiter == 0)
        {
            this.itemInspectionCount = 0;
            this.module.ChatManager.PrintInformationMessage("Rate limited, pausing item inspection loop.");
            return;
        }

        ClickItemInspectionResult.Using(addon).Next();
    }
}