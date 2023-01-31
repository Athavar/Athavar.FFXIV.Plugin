// <copyright file="ClickRetainerTaskAsk.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon RetainerTaskAsk.
/// </summary>
public sealed unsafe class ClickRetainerTaskAsk : ClickBase<ClickRetainerTaskAsk, AddonRetainerTaskAsk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickRetainerTaskAsk" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickRetainerTaskAsk(nint addon = default)
        : base("RetainerTaskAsk", addon)
    {
    }

    public static implicit operator ClickRetainerTaskAsk(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRetainerTaskAsk Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the assign button.
    /// </summary>
    [ClickName("retainer_venture_ask_assign")]
    public void Assign() => this.ClickAddonButton(this.Addon->AssignButton, 1);

    /// <summary>
    ///     Click the return button.
    /// </summary>
    [ClickName("retainer_venture_ask_return")]
    public void Return() => this.ClickAddonButton(this.Addon->ReturnButton, 2);
}