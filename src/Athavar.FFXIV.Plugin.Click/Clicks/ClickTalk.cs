// <copyright file="ClickTalk.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon Talk.
/// </summary>
public sealed class ClickTalk : ClickBase<ClickTalk, AddonTalk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickTalk" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickTalk(nint addon = default)
        : base("Talk", addon)
    {
    }

    public static implicit operator ClickTalk(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickTalk Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the talk dialog.
    /// </summary>
    [ClickName("talk")]
    public void Click() => this.ClickAddonStage(0);
}