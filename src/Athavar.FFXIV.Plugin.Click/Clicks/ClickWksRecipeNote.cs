// <copyright file="ClickWksRecipeNote.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Click.Clicks;

using Athavar.FFXIV.Plugin.Click.Attributes;
using Athavar.FFXIV.Plugin.Click.Bases;
using FFXIVClientStructs.FFXIV.Client.UI;

public sealed unsafe class ClickWksRecipeNote : ClickBase<ClickRecipeNote, AddonTalk>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickWksRecipeNote"/> class.
    /// </summary>
    /// <param name="addon">Addon pointer.</param>
    public ClickWksRecipeNote(IntPtr addon = default)
        : base("WKSRecipeNotebook", addon)
    {
    }

    public static implicit operator ClickWksRecipeNote(nint addon) => new(addon);

    /// <summary>
    ///     Instantiate this click using the given addon.
    /// </summary>
    /// <param name="addon">Addon to reference.</param>
    /// <returns>A click instance.</returns>
    public static ClickWksRecipeNote Using(nint addon) => new(addon);

    /// <summary>
    ///     Select a recipe entry.
    /// </summary>
    public void SelectRecipeEntry(uint index) => this.FireCallback(0, index);

    /// <summary>
    ///     Click the synthesize button.
    /// </summary>
    [ClickName("wks_synthesize")]
    public void Synthesize() => this.ClickAddonButton(this.Addon->GetComponentButtonById(50));

    /// <summary>
    ///     Click the ingredient nq button.
    /// </summary>
    [ClickName("wks_ingredient_nq")]
    public void IngredientNq() => this.ClickAddonButton(this.Addon->GetComponentButtonById(39));

    /// <summary>
    ///     Click the ingredient hq button.
    /// </summary>
    [ClickName("wks_ingredient_hq")]
    public void IngredientHq() => this.ClickAddonButton(this.Addon->GetComponentButtonById(40));
}