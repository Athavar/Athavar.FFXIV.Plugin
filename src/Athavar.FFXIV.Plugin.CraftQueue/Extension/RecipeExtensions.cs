// <copyright file="RecipeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Extension;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Recipe = Lumina.Excel.Sheets.Recipe;

internal static class RecipeExtensions
{
    public static RecipeExtended ToCraftSimulatorRecipe(this Recipe recipe, ExcelSheet<Item> sheets)
    {
        var lvlTable = recipe.RecipeLevelTable.ValueNullable ?? throw new AthavarPluginException();
        var maxQuality = (lvlTable.Quality * recipe.QualityFactor) / 100;

        Ingredient[] ingredients = recipe.Ingredient.Zip(recipe.AmountIngredient).Select(
            pack =>
            {
                var i = pack.First;
                var amount = pack.Second;
                if (i.ValueNullable is not { } item || item.RowId == 0)
                {
                    return null;
                }

                return new Ingredient(item.RowId, item.Icon, item.LevelItem.RowId, amount) { CanBeHq = item.CanBeHq };
            }).Where(i => i is not null).ToArray()!;
        var totalItemLevel = ingredients.Sum(i => i.CanBeHq ? i.Amount * i.ILevel : 0);
        var totalContribution = (maxQuality * recipe.MaterialQualityFactor) / 100;

        for (var index = 0; index < ingredients.Length; index++)
        {
            var ingredient = ingredients[index];
            if (ingredient.CanBeHq)
            {
                ingredient.Quality = (uint)(((float)ingredient.ILevel / totalItemLevel) * totalContribution);
            }
        }

        return new RecipeExtended
        {
            RecipeId = recipe.RowId,
            RecipeLevel = lvlTable.RowId,
            Level = lvlTable.ClassJobLevel,
            MaxQuality = maxQuality,
            Durability = (lvlTable.Durability * recipe.DurabilityFactor) / 100,
            Progress = ((uint)lvlTable.Difficulty * recipe.DifficultyFactor) / 100,
            ProgressDivider = lvlTable.ProgressDivider,
            QualityDivider = lvlTable.QualityDivider,
            ProgressModifier = lvlTable.ProgressModifier,
            QualityModifier = lvlTable.QualityModifier,
            Class = (CraftingClass)recipe.CraftType.RowId,
            Expert = recipe.IsExpert,
            CraftsmanshipReq = recipe.RequiredCraftsmanship,
            ControlReq = recipe.RequiredControl,
            QualityReq = recipe.RequiredQuality,
            ItemReq = recipe.ItemRequired.RowId,
            PossibleConditions = Enum.GetValues<StepState>().Where(f => f != StepState.NONE).Where(f => (f & (StepState)lvlTable.ConditionsFlag) == f).ToArray(),
            Ingredients = ingredients,
            ResultItemName = recipe.ItemResult.ValueNullable?.Name.ToDalamudString().ToString(),
            RequiredItemName = recipe.ItemRequired.ValueNullable?.Name.ToDalamudString().ToString(),
        };
    }
}