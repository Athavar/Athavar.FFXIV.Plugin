// <copyright file="RecipeNodeHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.RecipeNodes;

using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftQueue.Interfaces;
using Athavar.FFXIV.Plugin.CraftQueue.Job;

internal class RecipeNodeHandler : IRecipeNodeHandler
{
    public string AddonName => Constants.Addons.RecipeNote;

    public int OpenRecipe(IBaseCraftingJob job, CraftQueue queue)
    {
        var ci = queue.CommandInterface;
        var recipeId = job.Recipe.RecipeId;

        var selectedRecipeItemId = ci.GetRecipeNoteSelectedRecipeId();
        if ((!ci.IsAddonVisible(this.AddonName) && !ci.IsAddonVisible(Constants.Addons.Synthesis)) || (ci.IsAddonVisible(this.AddonName) && (selectedRecipeItemId == -1 || recipeId != selectedRecipeItemId)))
        {
            ci.OpenRecipeByRecipeId(recipeId);
            return -500;
        }

        return !ci.IsAddonVisible(this.AddonName) ? -1000 : 100;
    }

    public int SelectIngredients(IBaseCraftingJob job, CraftQueue queue)
    {
        var ci = queue.CommandInterface;
        if (!ci.IsAddonVisible(this.AddonName))
        {
            return -100;
        }

        var ptr = queue.DalamudServices.GameGui.GetAddonByName(this.AddonName);
        if (ptr == nint.Zero)
        {
            return -100;
        }

        var click = ClickRecipeNote.Using(ptr);

        click.MaterialNq();
        if (job.HqIngredients.Length != 0)
        {
            for (var idx = 0; idx < job.Recipe.Ingredients.Length; ++idx)
            {
                var ingredient = job.Recipe.Ingredients[idx];
                var found = job.HqIngredients.FirstOrDefault(i => i.ItemId == ingredient.Id);
                if (found != default)
                {
                    for (var index = 0; index < found.Amount; ++index)
                    {
                        click.Material(idx, true);
                    }
                }
            }
        }

        return 0;
    }

    public int StartCraft(IBaseCraftingJob job, CraftQueue queue)
    {
        var ci = queue.CommandInterface;
        if (!ci.IsAddonVisible(this.AddonName))
        {
            return -100;
        }

        var ptr = queue.DalamudServices.GameGui.GetAddonByName(this.AddonName);
        if (ptr == nint.Zero)
        {
            return -100;
        }

        var click = ClickRecipeNote.Using(ptr);

        if (job.Flags.HasFlag(CraftingJobFlags.TrialSynthesis))
        {
            click.TrialSynthesis();
        }
        else
        {
            click.Synthesize();
        }

        return 0;
    }
}