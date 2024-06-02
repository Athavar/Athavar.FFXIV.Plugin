// <copyright file="SimulationFailCause.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable InconsistentNaming
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

#pragma warning disable SA1602
#pragma warning disable SA1602
public enum SimulationFailCause
{
    /// <summary>
    ///     Only used for safe mode, this is for when safe mode is enabled and action success rate is &lt; 100 at this moment.
    /// </summary>
    UNSAFE_ACTION,
    INVALID_ACTION,
    DURABILITY_REACHED_ZERO,
    NOT_ENOUGH_CP,
    MISSING_LEVEL_REQUIREMENT,
    MISSING_STATS_REQUIREMENT,
    NOT_SPECIALIST,
    NO_INNER_QUIET,
    QUALITY_TOO_LOW,
}
#pragma warning restore SA1602