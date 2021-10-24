﻿// <copyright file="ClickItemInspectionResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace ClickLib.Clicks
{
    using System;

    using FFXIVClientStructs.FFXIV.Client.UI;
    using FFXIVClientStructs.FFXIV.Component.GUI;

    /// <summary>
    /// Addon ItemInspectionResult.
    /// </summary>
    public sealed unsafe class ClickItemInspectionResult : ClickAddonBase<AddonItemInspectionResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClickItemInspectionResult"/> class.
        /// </summary>
        /// <param name="addon">Addon pointer.</param>
        public ClickItemInspectionResult(IntPtr addon = default)
            : base(addon)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "ItemInspectionResult";

        public static implicit operator ClickItemInspectionResult(IntPtr addon) => new(addon);

        /// <summary>
        /// Instantiate this click using the given addon.
        /// </summary>
        /// <param name="addon">Addon to reference.</param>
        /// <returns>A click instance.</returns>
        public static ClickItemInspectionResult Using(IntPtr addon) => new(addon);

        /// <summary>
        /// Click the next button.
        /// </summary>
        [ClickName("item_inspection_result_next")]
        public void Next()
        {
            ClickAddonButton(&this.Addon->AtkUnitBase, (AtkComponentButton*)this.Addon->AtkUnitBase.UldManager.NodeList[2], 0);
        }

        /// <summary>
        /// Click the close button.
        /// </summary>
        [ClickName("item_inspection_result_close")]
        public void Close()
        {
            ClickAddonButton(&this.Addon->AtkUnitBase, (AtkComponentButton*)this.Addon->AtkUnitBase.UldManager.NodeList[3], 0xFFFF_FFFF);
        }
    }
}
