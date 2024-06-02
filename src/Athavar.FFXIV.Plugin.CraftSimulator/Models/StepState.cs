// <copyright file="StepState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable InconsistentNaming

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

[Flags]
public enum StepState : uint
{
    /// <summary>
    ///     Fails the step.
    /// </summary>
    NONE,

    /// <summary>
    ///     Default state.
    /// </summary>
    NORMAL = 0x01,

    /// <summary>
    ///     Quality multiplication of 1.5x for next command.
    /// </summary>
    GOOD = 0x02,

    /// <summary>
    ///     Quality multiplication of 2x for next command. Not available on expert recipes.
    /// </summary>
    EXCELLENT = 0x04, // Not available on expert recipes

    /// <summary>
    ///     Quality multiplication of 0.5x for next command. Not available on expert recipes.
    /// </summary>
    POOR = 0x08,

    /// <summary>
    ///     Only for expert recipes. Increase success rate by 25%.
    /// </summary>
    CENTERED = 0x10,

    /// <summary>
    ///     Only for expert recipes. Reduces loss of durability by 50%, stacks with WN & WN2.
    /// </summary>
    STURDY = 0x20,

    /// <summary>
    ///     Only for expert recipes. Reduces CP cost by 50%.
    /// </summary>
    PLIANT = 0x40,

    /// <summary>
    ///     Only for super expert recipes. Good, but for Progress. Doesn't proc Intensive/Precise.
    /// </summary>
    MALLEABLE = 0x80,

    /// <summary>
    ///     Only for super expert recipes. Next status is +2 duration.
    /// </summary>
    PRIMED = 0x100,

    /// <summary>
    ///     // Next step is GOOD condition.
    /// </summary>
    GOOD_OMEN = 0x200,
}