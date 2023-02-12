// <copyright file="ClickTitleMenu.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;

public sealed class ClickTitleMenu : ClickBase<ClickTitleMenu>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickTitleMenu" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickTitleMenu(nint addon = default)
        : base("_TitleMenu", addon)
    {
    }

    public static implicit operator ClickTitleMenu(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickTitleMenu Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the cancel button.
    /// </summary>
    [ClickName("login_start")]
    public void Start() => this.FireCallback(1, 0, 0);
}