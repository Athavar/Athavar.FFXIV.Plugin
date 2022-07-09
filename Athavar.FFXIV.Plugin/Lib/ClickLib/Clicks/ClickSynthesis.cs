// <copyright file="ClickSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;

using System;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Attributes;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Bases;
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
    public ClickSynthesis(IntPtr addon = default)
        : base("Synthesis", addon)
    {
    }

    public static implicit operator ClickSynthesis(IntPtr addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickRecipeNote Using(IntPtr addon) => new(addon);

    /// <summary>
    ///     Click the quit button.
    /// </summary>
    [ClickName("synthesis_quit")]
    public void Quit() => this.ClickAddonButton(this.Addon->QuitButton, 0); // Callback(-1)
}