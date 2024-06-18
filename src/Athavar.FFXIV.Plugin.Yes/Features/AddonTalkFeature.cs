// <copyright file="AddonTalkFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonTalk feature.
/// </summary>
internal class AddonTalkFeature : OnSetupFeature
{
    private ClickTalk? clickTalk;
    private nint lastTalkAddon = nint.Zero;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonTalkFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonTalkFeature(YesModule module)
        : base(module, AddonEvent.PostDraw)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "Talk";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.FunctionEnabled;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        var addonPtr = (AddonTalk*)addon;
        if (!addonPtr->AtkUnitBase.IsVisible)
        {
            return;
        }

        var target = this.Module.DalamudServices.TargetManager.Target;
        var targetName = target != null
            ? this.Module.GetSeStringText(target.Name)
            : string.Empty;

        if (this.Module.LastSeenTalkTarget != targetName)
        {
            this.Module.LastSeenTalkTarget = targetName;
            this.Module.LastSeenTargetSkip = false;
            if (!string.IsNullOrEmpty(targetName))
            {
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

                    this.Module.LastSeenTargetSkip = true;
                    break;
                }
            }
        }

        if (this.Module.LastSeenTargetSkip)
        {
            this.Click(addon);
        }
    }

    private void Click(IntPtr addon)
    {
        if (this.clickTalk == null || this.lastTalkAddon != addon)
        {
            this.clickTalk = ClickTalk.Using(this.lastTalkAddon = addon);
        }

        this.Module.Logger.Debug("AddonTalk: Advancing");
        this.clickTalk.Click();
    }

    private bool EntryMatchesTargetName(TalkEntryNode node, string targetName)
        => node.TargetIsRegex && (node.TargetRegex.Value?.IsMatch(targetName) ?? false) ||
           !node.TargetIsRegex && targetName.Contains(node.TargetText);
}