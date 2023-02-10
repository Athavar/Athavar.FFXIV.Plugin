// <copyright file="SpearFishSizeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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