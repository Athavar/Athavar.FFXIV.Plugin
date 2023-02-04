// <copyright file="SpearfishSpeed.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear.Enum;

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