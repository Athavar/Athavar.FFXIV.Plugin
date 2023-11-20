// <copyright file="ICommandInterface.Crafting.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces;

public partial interface ICommandInterface
{
    /// <summary>
    ///     Get a value indicating whether the player is crafting.
    /// </summary>
    /// <returns>True or false.</returns>
    public bool IsCrafting();

    /// <summary>
    ///     Get a value indicating whether the player is not crafting.
    /// </summary>
    /// <returns>True or false.</returns>
    public bool IsNotCrafting();

    /// <summary>
    ///     Get a value indicating whether the current craft is collectable.
    /// </summary>
    /// <returns>A value indicating whether the current craft is collectable.</returns>
    public bool IsCollectable();

    /// <summary>
    ///     Get the current synthesis condition.
    /// </summary>
    /// <param name="lower">A value indicating whether the result should be lowercased.</param>
    /// <returns>The current synthesis condition.</returns>
    public string GetCondition(bool lower = true);

    /// <summary>
    ///     Get a value indicating whether the current condition is active.
    /// </summary>
    /// <param name="condition">The condition to check.</param>
    /// <param name="lower">A value indicating whether the result should be lowercased.</param>
    /// <returns>A value indcating whether the current condition is active.</returns>
    public bool HasCondition(string condition, bool lower = true);

    /// <summary>
    ///     Get the current progress value.
    /// </summary>
    /// <returns>The current progress value.</returns>
    public int GetProgress();

    /// <summary>
    ///     Get the max progress value.
    /// </summary>
    /// <returns>The max progress value.</returns>
    public int GetMaxProgress();

    /// <summary>
    ///     Get a value indicating whether max progress has been achieved.
    ///     This is useful when a crafting animation is finishing.
    /// </summary>
    /// <returns>A value indicating whether max progress has been achieved.</returns>
    public bool HasMaxProgress();

    /// <summary>
    ///     Get the current quality value.
    /// </summary>
    /// <returns>The current quality value.</returns>
    public int GetQuality();

    /// <summary>
    ///     Get the max quality value.
    /// </summary>
    /// <returns>The max quality value.</returns>
    public int GetMaxQuality();

    /// <summary>
    ///     Get a value indicating whether max quality has been achieved.
    /// </summary>
    /// <returns>A value indicating whether max quality has been achieved.</returns>
    public bool HasMaxQuality();

    /// <summary>
    ///     Get the current durability value.
    /// </summary>
    /// <returns>The current durability value.</returns>
    public int GetDurability();

    /// <summary>
    ///     Get the max durability value.
    /// </summary>
    /// <returns>The max durability value.</returns>
    public int GetMaxDurability();

    /// <summary>
    ///     Get the current CP amount.
    /// </summary>
    /// <returns>The current CP amount.</returns>
    public int GetCp();

    /// <summary>
    ///     Get the max CP amount.
    /// </summary>
    /// <returns>The max CP amount.</returns>
    public int GetMaxCp();

    /// <summary>
    ///     Get the current step value.
    /// </summary>
    /// <returns>The current step value.</returns>
    public int GetStep();

    /// <summary>
    ///     Get the current percent HQ (and collectable) value.
    /// </summary>
    /// <returns>The current percent HQ value.</returns>
    public int GetPercentHQ();

    /// <summary>
    ///     Open the RecipeNode addon by recipe.
    /// </summary>
    /// <param name="recipeId">The recipe id.</param>
    public void OpenRecipeByRecipeId(uint recipeId);

    /// <summary>
    ///     Open the current selected recipe id of the RecipeNode addon.
    /// </summary>
    /// <returns>returns the recipeId. May return -1 if the addon is closes.</returns>
    public int GetRecipeNoteSelectedRecipeId();
}