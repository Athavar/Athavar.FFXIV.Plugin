// <copyright file="SpearFishSpeedExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.AutoSpear.Extensions;

using Athavar.FFXIV.Plugin.AutoSpear.Enum;

internal static class SpearFishSpeedExtensions
{
    public static string ToName(this SpearfishSpeed speed)
        => speed switch
           {
               SpearfishSpeed.Unknown => "Unknown Speed",
               SpearfishSpeed.SuperSlow => "Super Slow",
               SpearfishSpeed.ExtremelySlow => "Extremely Slow",
               SpearfishSpeed.VerySlow => "Very Slow",
               SpearfishSpeed.Slow => "Slow",
               SpearfishSpeed.Average => "Average",
               SpearfishSpeed.Fast => "Fast",
               SpearfishSpeed.VeryFast => "Very Fast",
               SpearfishSpeed.ExtremelyFast => "Extremely Fast",
               SpearfishSpeed.SuperFast => "Super Fast",
               SpearfishSpeed.HyperFast => "Hyper Fast",
               SpearfishSpeed.LynFast => "Mega Fast",
               SpearfishSpeed.None => "No Speed",
               _ => $"{(ushort)speed}",
           };
}