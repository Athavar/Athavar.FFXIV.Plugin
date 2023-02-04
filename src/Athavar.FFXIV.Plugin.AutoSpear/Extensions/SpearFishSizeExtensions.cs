// <copyright file="SpearFishSizeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.AutoSpear.Extensions;

using Athavar.FFXIV.Plugin.AutoSpear.Enum;

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