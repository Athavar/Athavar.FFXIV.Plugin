// <copyright file="GearsetExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Extension;

using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;

public static class GearsetExtensions
{
    public static CrafterStats ToCrafterStats(this Gearset gearset) => new(gearset.JobLevel, gearset.Control, gearset.Craftsmanship, gearset.CP, gearset.HasSoulStone);

    public static CraftingClass? GetCraftingJob(this Gearset gearset)
    {
        if ((int)gearset.JobClass is >= 8 and < 16)
        {
            return (CraftingClass)(gearset.JobClass - 8);
        }

        return null;
    }
}