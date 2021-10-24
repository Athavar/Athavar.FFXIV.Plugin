﻿// <copyright file="ClickRetainerTaskAsk.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace ClickLib.Clicks
{
    using System;

    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// Addon RetainerTaskAsk.
    /// </summary>
    public sealed unsafe class ClickRetainerTaskAsk : ClickAddonBase<AddonRetainerTaskAsk>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClickRetainerTaskAsk"/> class.
        /// </summary>
        /// <param name="addon">Addon pointer.</param>
        public ClickRetainerTaskAsk(IntPtr addon = default)
            : base(addon)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "RetainerTaskAsk";

        public static implicit operator ClickRetainerTaskAsk(IntPtr addon) => new(addon);

        /// <summary>
        /// Instantiate this click using the given addon.
        /// </summary>
        /// <param name="addon">Addon to reference.</param>
        /// <returns>A click instance.</returns>
        public static ClickRetainerTaskAsk Using(IntPtr addon) => new(addon);

        /// <summary>
        /// Click the assign button.
        /// </summary>
        [ClickName("retainer_venture_ask_assign")]
        public void Assign()
        {
            ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->AssignButton, 1);
        }

        /// <summary>
        /// Click the return button.
        /// </summary>
        [ClickName("retainer_venture_ask_return")]
        public void Return()
        {
            ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->ReturnButton, 2);
        }
    }
}
