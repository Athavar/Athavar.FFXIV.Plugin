// <copyright file="StepState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
// ReSharper disable InconsistentNaming
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using System;

[Flags]
internal enum StepState : ushort
{
    /// <summary>
    ///     Fails the step.
    /// </summary>
    NONE,

    /// <summary>
    ///     Default state.
    /// </summary>
    NORMAL = 0x00,

    /// <summary>
    ///     Quality multiplication of 1.5x for next command.
    /// </summary>
    GOOD = 0x01,

    /// <summary>
    ///     Quality multiplication of 2x for next command. Not available on expert recipes.
    /// </summary>
    EXCELLENT = 0x02, // Not available on expert recipes

    /// <summary>
    ///     Quality multiplication of 0.5x for next command. Not available on expert recipes.
    /// </summary>
    POOR = 0x04,

    /// <summary>
    ///     Only for expert recipes. Increase success rate by 25%.
    /// </summary>
    CENTERED = 0x08,

    /// <summary>
    ///     Only for expert recipes. Reduces loss of durability by 50%, stacks with WN & WN2.
    /// </summary>
    STURDY = 0x10,

    /// <summary>
    ///     Only for expert recipes. Reduces CP cost by 50%.
    /// </summary>
    PLIANT = 0x20,

    /// <summary>
    ///     Only for super expert recipes. Good, but for Progress. Doesn't proc Intensive/Precise.
    /// </summary>
    MALLEABLE = 0x40,

    /// <summary>
    ///     Only for super expert recipes. Next status is +2 duration.
    /// </summary>
    PRIMED = 0x80,
}