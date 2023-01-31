// <copyright file="AddonJournalResultFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonJournalResult feature.
/// </summary>
internal class AddonJournalResultFeature : OnSetupFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonJournalResultFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonJournalResultFeature(YesModule module)
        : base("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B EA 49 8B F0 BA ?? ?? ?? ?? 48 8B F9 E8 ?? ?? ?? ?? BA ?? ?? ?? ?? 48 89 87", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "JournalResult";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(nint addon, uint a2, nint data)
    {
        if (!this.Configuration.JournalResultCompleteEnabled)
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