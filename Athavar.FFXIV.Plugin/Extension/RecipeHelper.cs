// <copyright file="RecipeHelper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Extension;

using Athavar.FFXIV.Plugin.Utils.Constants;
using Lumina.Excel.GeneratedSheets;

internal static class RecipeHelper
{
    internal static Job GetJobType(this Recipe recipe)
    {
        Job jobType;
        switch (recipe.CraftType.Row)
        {
            case 0:
                jobType = Job.Carpenter;
                break;
            case 1:
                jobType = Job.Blacksmith;
                break;
            case 2:
                jobType = Job.Armorer;
                break;
            case 3:
                jobType = Job.Goldsmith;
                break;
            case 4:
                jobType = Job.Leatherworker;
                break;
            case 5:
                jobType = Job.Weaver;
                break;
            case 6:
                jobType = Job.Alchemist;
                break;
            case 7:
                jobType = Job.Culinarian;
                break;
            default:
                jobType = 0;
                break;
        }

        return jobType;
    }
}