// <copyright file="SpearfishSize.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using System;

internal enum SpearfishSize : byte
{
    Unknown = 0,
    Small = 1,
    Average = 2,
    Large = 3,
    None = 255,
}

internal static class SpearFishSizeExtensions
{
    public static string ToName(this SpearfishSize size)
        => size switch
           {
               SpearfishSize.Unknown => "Unknown Size",
               SpearfishSize.Small => "Small",
               SpearfishSize.Average => "Average",
               SpearfishSize.Large => "Large",
               SpearfishSize.None => "No Size",
               _ => throw new ArgumentOutOfRangeException(nameof(size), size, null),
           };
}