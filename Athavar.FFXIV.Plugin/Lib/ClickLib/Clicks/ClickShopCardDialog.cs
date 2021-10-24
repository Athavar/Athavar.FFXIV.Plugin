﻿// <copyright file="ClickShopCardDialog.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace ClickLib.Clicks
{
    using System;

    using FFXIVClientStructs.FFXIV.Client.UI;
    using FFXIVClientStructs.FFXIV.Component.GUI;

    /// <summary>
    /// Addon ShopCardDialog.
    /// </summary>
    public sealed unsafe class ClickShopCardDialog : ClickAddonBase<AddonShopCardDialog>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClickShopCardDialog"/> class.
        /// </summary>
        /// <param name="addon">Addon pointer.</param>
        public ClickShopCardDialog(IntPtr addon = default)
            : base(addon)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "ShopCardDialog";

        public static implicit operator ClickShopCardDialog(IntPtr addon) => new(addon);

        /// <summary>
        /// Instantiate this click using the given addon.
        /// </summary>
        /// <param name="addon">Addon to reference.</param>
        /// <returns>A click instance.</returns>
        public static ClickShopCardDialog Using(IntPtr addon) => new(addon);

        /// <summary>
        /// Click the sell button.
        /// </summary>
        [ClickName("sell_triple_triad_card")]
        public unsafe void Sell()
        {
            ClickAddonButton(&this.Addon->AtkUnitBase, (AtkComponentButton*)this.Addon->AtkUnitBase.UldManager.NodeList[3], 0);
        }
    }
}
