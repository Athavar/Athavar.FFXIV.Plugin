// <copyright file="AddonTalkFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonTalk feature.
/// </summary>
internal class AddonTalkFeature : UpdateFeature
{
    private readonly YesModule module;
    private ClickTalk? clickTalk;
    private nint lastTalkAddon = nint.Zero;

    private AddonEvent lastEvent = AddonEvent.PreFinalize;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonTalkFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonTalkFeature(YesModule module)
        : base(module, AddonEvent.PostDraw)
    {
        this.module = module;
        module.DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostRequestedUpdate, this.AddonName, this.TriggerHandler);
    }

    /// <inheritdoc/>
    protected override string AddonName => "Talk";

    public override void Dispose()
    {
        base.Dispose();
        this.module.DalamudServices.AddonLifecycle.UnregisterListener(AddonEvent.PostRequestedUpdate, this.AddonName, this.TriggerHandler);
    }

    /// <inheritdoc/>
    protected override unsafe void UpdateImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AddonTalk*)addon;
        if (!addonPtr->AtkUnitBase.IsVisible || addonEvent == this.lastEvent)
        {
            return;
        }

        this.lastEvent = addonEvent;

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

            if (this.clickTalk == null || this.lastTalkAddon != addon)
            {
                this.clickTalk = ClickTalk.Using(this.lastTalkAddon = addon);
            }

            this.module.Logger.Debug("AddonTalk: Advancing");
            this.clickTalk.Click();
            return;
        }
    }

    private bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        => (node.TargetIsRegex && (node.TargetRegex.Value?.IsMatch(targetName) ?? false)) ||
           (!node.TargetIsRegex && targetName.Contains(node.TargetText));
}