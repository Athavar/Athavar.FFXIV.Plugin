// <copyright file="SpearfishSpeed.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

internal enum SpearfishSpeed : ushort
{
    Unknown = 0,
    SuperSlow = 100,
    ExtremelySlow = 150,
    VerySlow = 200,
    Slow = 250,
    Average = 300,
    Fast = 350,
    VeryFast = 400,
    ExtremelyFast = 450,
    SuperFast = 500,
    HyperFast = 550,
    LynFast = 600,

    None = ushort.MaxValue,
}

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