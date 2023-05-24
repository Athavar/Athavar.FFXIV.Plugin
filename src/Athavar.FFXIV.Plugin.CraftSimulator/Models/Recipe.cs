// <copyright file="Recipe.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public sealed class Recipe
{
    public Recipe(
        uint id,
        uint recipeLevel,
        uint maxQuality,
        uint progress,
        int durability,
        byte progressDivider,
        byte qualityDivider,
        byte progressModifier,
        byte qualityModifier,
        CraftingClass craftingClass = CraftingClass.ANY,
        int level = 90,
        bool isExpert = false,
        int requiredCraftsmanship = 0,
        int requiredControl = 0,
        uint requiredQuality = 0,
        StepState[]? possibleConditions = null,
        Ingredient[]? ingredients = null)
    {
        this.RecipeId = id;
        this.RecipeLevel = recipeLevel;
        this.Level = level;
        this.Class = craftingClass;

        this.MaxQuality = maxQuality;
        this.Progress = progress;
        this.Durability = durability;

        this.Expert = isExpert;

        this.CraftsmanshipReq = requiredCraftsmanship == 0 ? null : requiredCraftsmanship;
        this.ControlReq = requiredControl == 0 ? null : requiredControl;
        this.QualityReq = requiredQuality == 0 ? null : requiredQuality;

        this.PossibleConditions = possibleConditions ?? new[] { StepState.NORMAL, StepState.GOOD, StepState.EXCELLENT, StepState.POOR };

        this.ProgressDivider = progressDivider;
        this.QualityDivider = qualityDivider;
        this.ProgressModifier = progressModifier;
        this.QualityModifier = qualityModifier;

        this.Ingredients = ingredients ?? Array.Empty<Ingredient>();
    }

    /// <summary>
    ///     Gets the recipe id.
    /// </summary>
    public uint RecipeId { get; }

    /// <summary>
    ///     Gets the lvl.
    /// </summary>
    public int Level { get; }

    /// <summary>
    ///     Gets the rlvl.
    /// </summary>
    public uint RecipeLevel { get; }

    /// <summary>
    ///     Gets the max quality of the recipe.
    /// </summary>
    public uint MaxQuality { get; }

    /// <summary>
    ///     Gets or sets the progress of the recipe.
    /// </summary>
    public uint Progress { get; }

    /// <summary>
    ///     Gets or sets the durability of the recipe.
    /// </summary>
    public int Durability { get; }

    public bool Expert { get; }

    public int? CraftsmanshipReq { get; }

    public int? ControlReq { get; }

    public uint? QualityReq { get; }

    public StepState[] PossibleConditions { get; }

    public byte ProgressDivider { get; }

    public byte QualityDivider { get; }

    public byte ProgressModifier { get; }

    public byte QualityModifier { get; }

    public Ingredient[] Ingredients { get; }

    /// <summary>
    ///     Gets or sets the result item name of the recipe.
    /// </summary>
    public string? ResultItemName { get; set; } = null;

    public CraftingClass Class { get; set; }
}