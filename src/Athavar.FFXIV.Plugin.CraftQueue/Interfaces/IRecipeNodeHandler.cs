// <copyright file="IRecipeNodeHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.Interfaces;

internal interface IRecipeNodeHandler
{
    string AddonName { get; }

    int OpenRecipe(IBaseCraftingJob job, CraftQueue queue);

    int SelectIngredients(IBaseCraftingJob job, CraftQueue queue);

    int StartCraft(IBaseCraftingJob job, CraftQueue queue);
}