// <copyright file="AddonContentsFinderConfirmFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;

/// <summary>
///     AddonContentsFinderConfirm feature.
/// </summary>
internal class AddonContentsFinderConfirmFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonContentsFinderConfirmFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonContentsFinderConfirmFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "ContentsFinderConfirm";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.ContentsFinderOneTimeConfirmEnabled;

    /// <inheritdoc/>
    protected override void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        ClickContentsFinderConfirm.Using(addon).Commence();

        if (this.Configuration.ContentsFinderOneTimeConfirmEnabled)
        {
            this.Configuration.ContentsFinderConfirmEnabled = false;
            this.Configuration.ContentsFinderOneTimeConfirmEnabled = false;
            this.Configuration.Save();
            this.UpdateEnableState();
        }
    }
}