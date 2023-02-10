// <copyright file="CraftingCondition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.CraftingData;

/// <summary>
///     Crafting condition types.
/// </summary>
internal enum CraftingCondition : uint
{
    /// <summary>
    ///     Normal condition.
    /// </summary>
    Normal = 1,

    /// <summary>
    ///     Good condition.
    /// </summary>
    Good = 2,

    /// <summary>
    ///     Excellent condition.
    /// </summary>
    Excellent = 3,

    /// <summary>
    ///     Poor condition.
    /// </summary>
    Poor = 4,

    /// <summary>
    ///     Centered condition.
    /// </summary>
    Centered = 5,

    /// <summary>
    ///     Sturdy condition.
    /// </summary>
    Sturdy = 6,

    /// <summary>
    ///     Pliant condition.
    /// </summary>
    Pliant = 7,

    /// <summary>
    ///     Malleable condition.
    /// </summary>
    Malleable = 8,

    /// <summary>
    ///     Primed condition.
    /// </summary>
    Primed = 9,
}