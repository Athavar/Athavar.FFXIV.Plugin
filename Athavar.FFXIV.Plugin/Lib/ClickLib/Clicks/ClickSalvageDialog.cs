﻿// <copyright file="ClickSalvageDialog.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Adddon SalvageDialog.
/// </summary>
public sealed unsafe class ClickSalvageDialog : ClickAddonBase<AddonSalvageDialog>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickSalvageDialog" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickSalvageDialog(IntPtr addon = default)
        : base(addon)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "SalvageDialog";

    public static implicit operator ClickSalvageDialog(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickSalvageDialog Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the desynthesize button.
    /// </summary>
    [ClickName("desynthesize")]
    public void Desynthesize() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->DesynthesizeButton, 1);

    /// <summary>
    ///     Click the desynthesize checkbox button.
    /// </summary>
    [ClickName("desynthesize_checkbox")]
    public void CheckBox() => ClickAddonCheckBox(&this.Addon->AtkUnitBase, this.Addon->CheckBox, 3);
}