// <copyright file="AddonSelectIconStringFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonSelectString feature.
/// </summary>
internal class AddonSelectIconStringFeature : OnSetupSelectListFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSelectIconStringFeature"/> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule"/>.</param>
    /// <param name="services">ServiceContainer of all dalamud services.</param>
    public AddonSelectIconStringFeature(YesModule module)
        : base("40 53 56 57 41 54 41 57 48 83 EC 30 4D 8B F8 44 8B E2 48 8B F1 E8 ?? ?? ?? ??", module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SelectIconString";

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(nint addon, uint a2, nint data)
    {
        var addonPtr = (AddonSelectIconString*)addon;
        var popupMenu = &addonPtr->PopupMenu.PopupMenu;

        this.SetupOnItemSelectedHook(popupMenu);
        this.CompareNodesToEntryTexts(addon, popupMenu);
    }

    /// <inheritdoc/>
    protected override void SelectItemExecute(nint addon, int index)
    {
        this.module.Logger.Debug($"AddonSelectIconString: Selecting {index}");
        ClickSelectIconString.Using(addon).SelectItem((ushort)index);
    }
}