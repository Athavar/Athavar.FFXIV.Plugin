// <copyright file="ClickRetainerTaskResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon RetainerTaskResult.
/// </summary>
public sealed unsafe class ClickRetainerTaskResult : ClickBase<ClickRetainerTaskResult, AddonRetainerTaskResult>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickRetainerTaskResult" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickRetainerTaskResult(nint addon = default)
        : base("RetainerTaskResult", addon)
    {
    }

    public static implicit operator ClickRetainerTaskResult(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRetainerTaskResult Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the confirm button.
    /// </summary>
    [ClickName("retainer_venture_result_confirm")]
    public void Confirm() => this.ClickAddonButton(this.Addon->ConfirmButton, 2);

    /// <summary>
    ///     Click the reassign button.
    /// </summary>
    [ClickName("retainer_venture_result_reassign")]
    public void Reassign() => this.ClickAddonButton(this.Addon->ReassignButton, 3);
}