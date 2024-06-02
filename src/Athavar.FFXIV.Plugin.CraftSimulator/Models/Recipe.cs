// <copyright file="Recipe.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public class Recipe
{
    private readonly int? craftsmanshipReq;
    private readonly int? controlReq;
    private readonly uint? qualityReq;

    /// <summary>
    ///     Gets the recipe id.
    /// </summary>
    public required uint RecipeId { get; init; }

    /// <summary>
    ///     Gets the lvl.
    /// </summary>
    public int Level { get; init; } = 10;

    /// <summary>
    ///     Gets the rlvl.
    /// </summary>
    public required uint RecipeLevel { get; init; }

    /// <summary>
    ///     Gets the max quality of the recipe.
    /// </summary>
    public required uint MaxQuality { get; init; }

    /// <summary>
    ///     Gets the progress of the recipe.
    /// </summary>
    public required uint Progress { get; init; }

    /// <summary>
    ///     Gets the durability of the recipe.
    /// </summary>
    public required int Durability { get; init; }

    public required bool Expert { get; init; }

    public int? CraftsmanshipReq
    {
        get => this.craftsmanshipReq;
        init => this.craftsmanshipReq = value == 0 ? null : value;
    }

    public int? ControlReq
    {
        get => this.controlReq;
        init => this.controlReq = value == 0 ? null : value;
    }

    public uint? QualityReq
    {
        get => this.qualityReq;
        init => this.qualityReq = value == 0 ? null : value;
    }

    public StepState[] PossibleConditions { get; init; } = [StepState.NORMAL, StepState.GOOD, StepState.EXCELLENT, StepState.POOR];

    public required byte ProgressDivider { get; init; }

    public required byte QualityDivider { get; init; }

    public required byte ProgressModifier { get; init; }

    public required byte QualityModifier { get; init; }

    public required Ingredient[] Ingredients { get; init; }

    public required CraftingClass Class { get; set; }
}