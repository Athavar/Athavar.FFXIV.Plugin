// <copyright file="CraftingJobFlags.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Job;

[Flags]
public enum CraftingJobFlags
{
    /// <summary>
    ///     the Zero state.
    /// </summary>
    None = 0,

    /// <summary>
    ///     Force usage of food.
    /// </summary>
    ForceFood = 1 << 0,

    /// <summary>
    ///     Force usage of potion.
    /// </summary>
    ForcePotion = 1 << 1,

    /// <summary>
    ///     Force usage of potion.
    /// </summary>
    TrialSynthesis = 1 << 2,

    /// <summary>
    ///     Force usage of potion.
    /// </summary>
    CosmicSynthesis = 1 << 3,
}