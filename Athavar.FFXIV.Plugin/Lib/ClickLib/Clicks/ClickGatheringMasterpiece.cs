﻿// <copyright file="ClickGatheringMasterpiece.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace ClickLib.Clicks
{
    using System;

    using FFXIVClientStructs.FFXIV.Client.UI;

    /// <summary>
    /// Addon GatheringMasterpiece.
    /// </summary>
    public sealed unsafe class ClickGatheringMasterpiece : ClickAddonBase<AddonGatheringMasterpiece>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClickGatheringMasterpiece"/> class.
        /// </summary>
        /// <param name="addon">Addon pointer.</param>
        public ClickGatheringMasterpiece(IntPtr addon = default)
            : base(addon)
        {
        }

        /// <inheritdoc/>
        protected override string AddonName => "GatheringMasterpiece";

        public static implicit operator ClickGatheringMasterpiece(IntPtr addon) => new(addon);

        /// <summary>
        /// Instantiate this click using the given addon.
        /// </summary>
        /// <param name="addon">Addon to reference.</param>
        /// <returns>A click instance.</returns>
        public static ClickGatheringMasterpiece Using(IntPtr addon) => new(addon);

        /// <summary>
        /// Click the collect button.
        /// </summary>
        [ClickName("collect")]
        public void Collect()
        {
            ClickAddonDragDrop(&this.Addon->AtkUnitBase, this.Addon->CollectDragDrop, 112);
        }
    }
}
