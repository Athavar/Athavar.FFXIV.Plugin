// <copyright file="GearsetExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models;

public static class GearsetExtensions
{
    public static CrafterStats ToCrafterStats(this Gearset gearset) => new(gearset.JobLevel, gearset.Control, gearset.Craftsmanship, gearset.CP, gearset.HasSoulStone, gearset.HasSplendorousTools());

    public static CraftingClass? GetCraftingJob(this Gearset gearset)
    {
        if ((int)gearset.JobClass is >= 8 and < 16)
        {
            return (CraftingClass)(gearset.JobClass - 8);
        }

        return null;
    }

    public static bool HasSplendorousTools(this Gearset gearset)
        => gearset.MainHandItemId is 38737 or // crystalline saw
                                     38738 or // crystalline cross-pein hammer
                                     38739 or // crystalline raising hammer
                                     38740 or // crystalline mallet
                                     38741 or // crystalline round knife
                                     38742 or // crystalline needle
                                     38743 or // crystalline alembic
                                     38744 or // crystalline frypan
                                     38745 or // crystalline pickaxe
                                     38746 or // crystalline hatchet
                                     38747;   // crystalline fishing rod
}