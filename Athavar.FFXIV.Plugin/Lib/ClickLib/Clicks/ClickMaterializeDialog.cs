﻿// <copyright file="ClickMaterializeDialog.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Attributes;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon MaterializeDialog.
/// </summary>
public sealed unsafe class ClickMaterializeDialog : ClickBase<ClickMaterializeDialog, AddonMaterializeDialog>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickMaterializeDialog" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickMaterializeDialog(IntPtr addon = default)
        : base("MaterializeDialog", addon)
    {
    }

    public static implicit operator ClickMaterializeDialog(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickMaterializeDialog Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the deliver button.
    /// </summary>
    [ClickName("materialize")]
    public void Materialize() => this.ClickAddonButton(this.Addon->YesButton, 0);
}