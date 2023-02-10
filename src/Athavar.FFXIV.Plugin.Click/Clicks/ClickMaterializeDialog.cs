// <copyright file="ClickMaterializeDialog.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
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
    public ClickMaterializeDialog(nint addon = default)
        : base("MaterializeDialog", addon)
    {
    }

    public static implicit operator ClickMaterializeDialog(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickMaterializeDialog Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the deliver button.
    /// </summary>
    [ClickName("materialize")]
    public void Materialize() => this.ClickAddonButton(this.Addon->YesButton, 0);
}