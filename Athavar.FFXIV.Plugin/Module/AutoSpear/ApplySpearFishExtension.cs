// <copyright file="ApplySpearFishExtension.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using System.Collections.Generic;

internal static class ApplySpearFishExtension
{
    public static SpearFish? Apply(this IDictionary<uint, SpearFish> data, uint id, Patch patch)
    {
        if (data.TryGetValue(id, out var fish))
        {
            return fish;
        }

        return null;
    }

    public static SpearFish? Spear(this SpearFish? fish, SpearfishSize size, SpearfishSpeed speed = SpearfishSpeed.Unknown)
    {
        if (fish == null)
        {
            return null;
        }

        fish.Size = size == SpearfishSize.Unknown ? fish.Size : size;
        fish.Speed = speed == SpearfishSpeed.Unknown ? fish.Speed : speed;
        return fish;
    }

    public static SpearFish? Predators(this SpearFish? fish, IDictionary<uint, SpearFish> data, int intuitionLength, params (uint, int)[] predators)
        =>
            /* dummy*/
            fish;

    public static SpearFish? Time(this SpearFish? fish, int uptimeMinuteOfDayStart, int uptimeMinuteOfDayEnd)
        =>
            /* dummy*/
            fish;
}