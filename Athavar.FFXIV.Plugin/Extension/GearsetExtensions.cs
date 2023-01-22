// <copyright file="GearsetExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Extension;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Utils;

internal static class GearsetExtensions
{
    public static CrafterStats ToCrafterStats(this Gearset gearset) => new(gearset.JobLevel, gearset.Control, gearset.Craftsmanship, gearset.CP, gearset.HasSoulStone);
}