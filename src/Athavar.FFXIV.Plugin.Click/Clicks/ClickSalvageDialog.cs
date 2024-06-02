// <copyright file="ClickSalvageDialog.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Adddon SalvageDialog.
/// </summary>
public sealed unsafe class ClickSalvageDialog : ClickBase<ClickSalvageDialog, AddonSalvageDialog>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickSalvageDialog" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickSalvageDialog(nint addon = default)
        : base("SalvageDialog", addon)
    {
    }

    public static implicit operator ClickSalvageDialog(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickSalvageDialog Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the desynthesize button.
    /// </summary>
    [ClickName("desynthesize")]
    public void Desynthesize() => this.ClickAddonButton(this.Addon->DesynthesizeButton, 1);

    /// <summary>
    ///     Click the desynthesize checkbox button.
    /// </summary>
    [ClickName("desynthesize_checkbox")]
    public void CheckBox() => this.ClickAddonCheckBox(this.Addon->CheckBox, 3);
}