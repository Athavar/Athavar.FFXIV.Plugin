// <copyright file="Simulation.Config.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

internal partial class Simulation
{
    /// <summary>
    ///     Gets the current recipe to craft.
    /// </summary>
    public Recipe Recipe { get; }

    /// <summary>
    ///     Gets the current crafting stats for the recipe to craft.
    /// </summary>
    public CrafterStats CrafterStats { get; }

    /// <summary>
    ///     Gets the current crafting stats for the recipe to craft.
    /// </summary>
    public CrafterStats CurrentStats { get; private set; }

    /// <summary>
    ///     Gets or sets a value indicating whether everything should be linear (aka no fail on actions, Initial preparations
    ///     never procs).
    /// </summary>
    public bool Linear { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether all the actions have a success chances &lt; 100.
    /// </summary>
    public bool SafeMode { get; set; } = false;

    /// <summary>
    ///     Gets or sets applied stat modifiers.
    /// </summary>
    public StatModifiers?[]? CurrentStatModifiers { get; set; }
}