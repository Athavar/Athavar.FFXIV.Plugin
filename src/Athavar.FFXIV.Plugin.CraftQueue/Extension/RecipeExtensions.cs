// <copyright file="RecipeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Extension;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Recipe = Athavar.FFXIV.Plugin.CraftSimulator.Models.Recipe;

internal static class RecipeExtensions
{
    public static Recipe ToCraftSimulatorRecipe(this Lumina.Excel.GeneratedSheets.Recipe recipe, ExcelSheet<Item> sheets)
    {
        var lvlTable = recipe.RecipeLevelTable.Value ?? throw new AthavarPluginException();
        var maxQuality = (lvlTable.Quality * recipe.QualityFactor) / 100;

        Ingredient[] ingredients = recipe.UnkData5.Select(i =>
        {
            var item = sheets.GetRow((uint)i.ItemIngredient);
            if (item is null || item.RowId == 0)
            {
                return null;
            }

            return new Ingredient(item.RowId, item.Icon, item.LevelItem.Row, i.AmountIngredient) { CanBeHq = item.CanBeHq };
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

        return new Recipe(
            recipe.RowId,
            lvlTable.RowId,
            maxQuality,
            ((uint)lvlTable.Difficulty * recipe.DifficultyFactor) / 100,
            (lvlTable.Durability * recipe.DurabilityFactor) / 100,
            lvlTable.ProgressDivider,
            lvlTable.QualityDivider,
            lvlTable.ProgressModifier,
            lvlTable.QualityModifier,
            (CraftingClass)recipe.CraftType.Row,
            lvlTable.ClassJobLevel,
            recipe.IsExpert,
            recipe.RequiredCraftsmanship,
            recipe.RequiredControl,
            recipe.RequiredQuality,
            Enum.GetValues<StepState>().Where(f => (f & (StepState)lvlTable.ConditionsFlag) == f).ToArray(),
            ingredients)
        {
            ResultItemName = recipe.ItemResult.Value?.Name.ToDalamudString().ToString(),
        };
    }
}