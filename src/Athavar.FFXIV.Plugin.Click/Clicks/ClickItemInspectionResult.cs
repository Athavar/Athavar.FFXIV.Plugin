// <copyright file="ClickItemInspectionResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon ItemInspectionResult.
/// </summary>
public sealed class ClickItemInspectionResult : ClickBase<ClickItemInspectionResult, AddonItemInspectionResult>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickItemInspectionResult" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickItemInspectionResult(nint addon = default)
        : base("ItemInspectionResult", addon)
    {
    }

    public static implicit operator ClickItemInspectionResult(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickItemInspectionResult Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the next button.
    /// </summary>
    [ClickName("item_inspection_result_next")]
    public void Next() => this.ClickAddonButtonIndex(2, 0);

    /// <summary>
    ///     Click the close button.
    /// </summary>
    [ClickName("item_inspection_result_close")]
    public void Close() => this.ClickAddonButtonIndex(3, 0xFFFF_FFFF);
}