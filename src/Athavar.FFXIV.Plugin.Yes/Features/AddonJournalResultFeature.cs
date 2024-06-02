// <copyright file="AddonJournalResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;

/// <summary>
///     AddonJournalResult feature.
/// </summary>
internal class AddonJournalResultFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonJournalResultFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    public AddonJournalResultFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "JournalResult";

    /// <inheritdoc/>
    protected override bool ConfigurationEnableState => this.Configuration.JournalResultCompleteEnabled;

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
    {
        ClickJournalResult journalResult = addon;
        if (!journalResult.Addon->CompleteButton->IsEnabled)
        {
            return;
        }

        ClickJournalResult.Using(addon).Complete();
    }
}