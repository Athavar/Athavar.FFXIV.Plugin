// <copyright file="Recipe.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using System;
using System.Linq;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

internal class Recipe
{
    public Recipe(Lumina.Excel.GeneratedSheets.Recipe recipe, ExcelSheet<Item> sheets)
    {
        var lvlTable = recipe.RecipeLevelTable.Value;
        this.RecipeLevel = lvlTable.RowId;
        this.Level = lvlTable.ClassJobLevel;
        this.Job = (CraftingJob)recipe.CraftType.Row;

        this.MaxQuality = (lvlTable.Quality * recipe.QualityFactor) / 100;
        this.Progress = ((uint)lvlTable.Difficulty * recipe.DifficultyFactor) / 100;
        this.Durability = (lvlTable.Durability * recipe.DurabilityFactor) / 100;

        this.Expert = recipe.IsExpert;

        this.CraftsmanshipReq = recipe.RequiredCraftsmanship == 0 ? null : recipe.RequiredCraftsmanship;
        this.ControlReq = recipe.RequiredControl == 0 ? null : recipe.RequiredControl;
        this.QualityReq = recipe.RequiredQuality == 0 ? null : recipe.RequiredQuality;

        this.PossibleConditions =
            Enum.GetValues<StepState>()
               .Where(f => (f & (StepState)lvlTable.ConditionsFlag) == f)
               .ToArray();

        this.ProgressDivider = lvlTable.ProgressDivider;
        this.QualityDivider = lvlTable.QualityDivider;
        this.ProgressModifier = lvlTable.ProgressModifier;
        this.QualityModifier = lvlTable.QualityModifier;

        this.Ingredients = recipe.UnkData5.Select(i =>
        {
            var item = sheets.GetRow((uint)i.ItemIngredient)!;
            return new Ingredient { Id = item.RowId, ILevel = item.LevelItem.Row, Amount = i.AmountIngredient, CanBeHq = item.CanBeHq };
        }).ToArray();
        var totalItemLevel = this.Ingredients.Sum(i => i.CanBeHq ? i.Amount * i.ILevel : 0);
        var totalContribution = (this.MaxQuality * recipe.MaterialQualityFactor) / 100;
        foreach (var ingredient in this.Ingredients)
        {
            if (ingredient.CanBeHq)
            {
                ingredient.Quality = (uint)(ingredient.ILevel / totalItemLevel) * totalContribution;
            }
        }
    }

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

    public CraftingJob Job { get; set; }
}