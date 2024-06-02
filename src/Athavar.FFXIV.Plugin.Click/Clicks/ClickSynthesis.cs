// <copyright file="ClickSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Addon RecipeNote.
/// </summary>
public sealed unsafe class ClickSynthesis : ClickBase<ClickSynthesis, AddonSynthesis>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickSynthesis" /> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickSynthesis(nint addon = default)
        : base("Synthesis", addon)
    {
    }

    public static implicit operator ClickSynthesis(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRecipeNote Using(nint addon) => new(addon);

    /// <summary>
    ///     Click the quit button.
    /// </summary>
    [ClickName("synthesis_quit")]
    public void Quit() => this.ClickAddonButton(this.Addon->QuitButton, 0); // Callback(-1)
}