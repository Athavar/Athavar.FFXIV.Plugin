// <copyright file="ClickShopCardDialog.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Attributes;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon ShopCardDialog.
/// </summary>
public sealed class ClickShopCardDialog : ClickBase<ClickShopCardDialog, AddonShopCardDialog>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickShopCardDialog" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickShopCardDialog(IntPtr addon = default)
        : base("ShopCardDialog", addon)
    {
    }

    public static implicit operator ClickShopCardDialog(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickShopCardDialog Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the sell button.
    /// </summary>
    [ClickName("sell_triple_triad_card")]
    public void Sell() => this.ClickAddonButtonIndex(3, 0); // Callback(0, 0, 0)
}