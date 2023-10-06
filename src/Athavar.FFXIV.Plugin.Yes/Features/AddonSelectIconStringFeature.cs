// <copyright file="AddonSelectIconStringFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.Addon.Lifecycle;
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
    public AddonSelectIconStringFeature(YesModule module)
        : base(module)
    {
    }

    /// <inheritdoc/>
    protected override string AddonName => "SelectIconString";

    /// <inheritdoc/>
    protected override unsafe void OnSetupImpl(IntPtr addon, AddonEvent addonEvent)
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