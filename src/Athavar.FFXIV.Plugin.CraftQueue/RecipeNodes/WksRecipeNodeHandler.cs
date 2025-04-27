// <copyright file="WksRecipeNodeHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.RecipeNodes;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftQueue.Interfaces;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

internal class WksRecipeNodeHandler : IRecipeNodeHandler
{
    public string AddonName => Constants.Addons.WksRecipeNote;

    public unsafe int OpenRecipe(IBaseCraftingJob job, CraftQueue queue)
    {
        var ci = queue.CommandInterface;
        var recipeId = job.Recipe.RecipeId;

        var selectedRecipeItemId = ci.GetRecipeNoteSelectedRecipeId();
        var recipeNodeOpen = ci.IsAddonVisible(this.AddonName);
        if (!recipeNodeOpen && !ci.IsAddonVisible(Constants.Addons.Synthesis))
        {
            ci.OpenRecipeByRecipeId(recipeId);
            return -500;
        }

        if (recipeNodeOpen && recipeId != selectedRecipeItemId)
        {
            var list = RecipeNote.Instance()->RecipeList;
            for (uint i = 0; i < list->RecipeCount; i++)
            {
                if (list->Recipes[i].RecipeId == recipeId)
                {
                    var ptr = queue.DalamudServices.GameGui.GetAddonByName(Constants.Addons.WksRecipeNote);
                    if (ptr == nint.Zero)
                    {
                        return -100;
                    }

                    var click = ClickWksRecipeNote.Using(ptr);
                    click.SelectRecipeEntry(i);

                    return -100;
                }
            }

            return -500;
        }

        return !recipeNodeOpen ? -1000 : 100;
    }

    public int SelectIngredients(IBaseCraftingJob job, CraftQueue queue)
    {
        var ptr = queue.DalamudServices.GameGui.GetAddonByName(Constants.Addons.WksRecipeNote);
        if (ptr == nint.Zero)
        {
            return -100;
        }

        var click = ClickWksRecipeNote.Using(ptr);
        click.IngredientNq();
        click.IngredientHq();

        return 0;
    }

    public int StartCraft(IBaseCraftingJob job, CraftQueue queue)
    {
        var ci = queue.CommandInterface;
        if (!ci.IsAddonVisible(Constants.Addons.WksRecipeNote))
        {
            return -100;
        }

        var ptr = queue.DalamudServices.GameGui.GetAddonByName(Constants.Addons.WksRecipeNote);
        if (ptr == nint.Zero)
        {
            return -100;
        }

        var click = ClickWksRecipeNote.Using(ptr);

        click.Synthesize();
        return 0;
    }
}