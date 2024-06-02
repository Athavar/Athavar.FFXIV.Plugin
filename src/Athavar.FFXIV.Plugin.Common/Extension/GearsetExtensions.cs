// <copyright file="GearsetExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
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
        => gearset.MainHandItemId
            is >= 38737 and <= 38747 or // crystalline
               >= 39732 and <= 39742 or // Chora-Zoi's crystalline
               >= 39743 and <= 39753 or // brilliant
               >= 41180 and <= 41190 or // vrandtic
               >= 41191 and <= 41201;   // lodestar
}