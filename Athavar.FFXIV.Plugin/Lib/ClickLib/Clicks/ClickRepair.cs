// <copyright file="ClickRepair.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon Request.
/// </summary>
public sealed unsafe class ClickRepair : ClickAddonBase<AddonRepair>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickRepair" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickRepair(IntPtr addon = default)
        : base(addon)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "Repair";

    public static implicit operator ClickRepair(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRequest Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the repair all button.
    /// </summary>
    [ClickName("repair_all")]
    public void RepairAll() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->RepairAllButton, 0);
}