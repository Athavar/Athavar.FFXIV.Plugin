// <copyright file="AddonSelectStringFeature.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes.Features;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     AddonSelectString feature.
/// </summary>
internal class AddonSelectStringFeature : OnSetupSelectListFeature
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AddonSelectStringFeature" /> class.
    /// </summary>
    /// <param name="module"><see cref="YesModule" />.</param>
    public AddonSelectStringFeature(YesModule module)
        : base("40 53 56 57 41 54 41 55 41 57 48 83 EC 48 4D 8B F8 44 8B E2 48 8B F1 E8 ?? ?? ?? ??", module)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "SelectString";

    /// <inheritdoc />
    protected override unsafe void OnSetupImpl(nint addon, uint a2, nint data)
    {
        var addonPtr = (AddonSelectString*)addon;
        var popupMenu = &addonPtr->PopupMenu.PopupMenu;
        this.SetupOnItemSelectedHook(popupMenu);
        this.CompareNodesToEntryTexts(addon, popupMenu);
    }

    /// <inheritdoc />
    protected override void SelectItemExecute(nint addon, int index)
    {
        PluginLog.Debug($"AddonSelectString: Selecting {index}");
        ClickSelectString.Using(addon).SelectItem((ushort)index);
    }
}