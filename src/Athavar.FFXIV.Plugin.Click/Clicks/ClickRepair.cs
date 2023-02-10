// <copyright file="ClickRepair.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon Request.
/// </summary>
public sealed unsafe class ClickRepair : ClickBase<ClickRepair, AddonRepair>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickRepair" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickRepair(nint addon = default)
        : base("Repair", addon)
    {
    }

    public static implicit operator ClickRepair(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRequest Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the repair all button.
    /// </summary>
    [ClickName("repair_all")]
    public void RepairAll() => this.ClickAddonButton(this.Addon->RepairAllButton, 0);
}