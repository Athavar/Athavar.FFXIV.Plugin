// <copyright file="ClickSelectYesno.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon SelectYesNo.
/// </summary>
public sealed unsafe class ClickSelectYesNo : ClickBase<ClickSelectYesNo, AddonSelectYesno>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickSelectYesNo" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickSelectYesNo(nint addon = default)
        : base("SelectYesno", addon)
    {
    }

    public static implicit operator ClickSelectYesNo(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickSelectYesNo Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the yes button.
    /// </summary>
    [ClickName("select_yes")]
    public void Yes() => this.ClickAddonButton(this.Addon->YesButton, 0);

    /// <summary>
    ///     Click the no button.
    /// </summary>
    [ClickName("select_no")]
    public void No() => this.ClickAddonButton(this.Addon->NoButton, 1);

    /// <summary>
    ///     Click the confirm checkbox.
    /// </summary>
    [ClickName("select_confirm")]
    public void Confirm() => this.ClickAddonCheckBox(this.Addon->ConfirmCheckBox, 3);
}