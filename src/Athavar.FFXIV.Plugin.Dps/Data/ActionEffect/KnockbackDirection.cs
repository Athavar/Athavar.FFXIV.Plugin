// <copyright file="KnockbackDirection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;

internal enum KnockbackDirection
{
    AwayFromSource = 0, // direction = target-source
    Arg = 1, // direction = arg.degrees()
    Random = 2, // direction = random(0, 2pi)
    SourceForward = 3, // direction = src.direction
    SourceRight = 4, // direction = src.direction - pi/2
    SourceLeft = 5, // direction = src.direction + pi/2
}