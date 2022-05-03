// <copyright file="ClickContentsFinderConfirm.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon ContentsFinderConfirm.
/// </summary>
public sealed unsafe class ClickContentsFinderConfirm : ClickAddonBase<AddonContentsFinderConfirm>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickContentsFinderConfirm" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickContentsFinderConfirm(IntPtr addon = default)
        : base(addon)
    {
    }

    /// <inheritdoc />
    protected override string AddonName => "ContentsFinderConfirm";

    public static implicit operator ClickContentsFinderConfirm(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickContentsFinderConfirm Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the collect button.
    /// </summary>
    [ClickName("duty_commence")]
    public void Commence() => ClickAddonButton(&this.Addon->AtkUnitBase, this.Addon->CommenceButton, 0x8);
}