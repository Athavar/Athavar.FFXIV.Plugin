// <copyright file="ClickRequest.cs" company="Athavar">
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
public sealed unsafe class ClickRequest : ClickBase<ClickRequest, AddonRequest>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickRequest" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickRequest(nint addon = default)
        : base("Request", addon)
    {
    }

    public static implicit operator ClickRequest(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRequest Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the hand over button.
    /// </summary>
    [ClickName("request_hand_over")]
    public void HandOver() => this.ClickAddonButton(this.Addon->HandOverButton, 0);

    /// <summary>
    ///     Click the cancel button.
    /// </summary>
    [ClickName("request_cancel")]
    public void Cancel() => this.ClickAddonButton(this.Addon->CancelButton, 1);
}