// <copyright file="Simulation.State.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation;

using System;
using System.Collections.Generic;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Constants;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

/// <summary>
/// </summary>
internal partial class Simulation
{
    public List<ActionResult> Steps { get; } = new();

    /// <summary>
    ///     Gets or sets a value indicating whether the craft was a success.
    /// </summary>
    public bool? Success { get; set; }

    /// <summary>
    ///     Gets or sets the quality of the current craft.
    /// </summary>
    public long Quality { get; set; }

    /// <summary>
    ///     Gets or sets the progression of the current craft.
    /// </summary>
    public int Progression { get; set; }

    /// <summary>
    ///     Gets or sets the durability of the current craft.
    /// </summary>
    public int Durability { get; set; }

    /// <summary>
    ///     Gets or sets the available CP of the current craft.
    /// </summary>
    public int AvailableCP { get; set; }

    /// <summary>
    ///     Gets or sets the state of the current craft.
    /// </summary>
    public StepState State { get; set; }

    /// <summary>
    ///     Gets or sets the state of the current craft.
    /// </summary>
    public bool Safe { get; set; }

    public void Reset(StatModifiers[]? statModifiers = null)
    {
        this.Success = null;
        this.Progression = 0;
        this.Durability = this.Recipe.Durability;
        this.Quality = this.startingQuality;
        this.effectiveBuffs.Clear();
        this.CurrentStats = new CrafterStats(this.CrafterStats);
        if (statModifiers is not null)
        {
            this.CurrentStats.Apply(statModifiers);
        }

        this.Steps.Clear();

        this.AvailableCP = (int?)this.CurrentStats?.CP ?? 0;
        this.State = StepState.NORMAL;
        this.Safe = false;
    }

    public bool HasComboAvaiable<T>()
        where T : CraftingAction
    {
        for (var index = this.Steps.Count - 1; index >= 0; index--)
        {
            var step = this.Steps[index];
            if (step.Success == true && step.Skill.Action is T)
            {
                return true;
            }

            // If there's an action that isn't skipped (fail or not), combo is broken
            if (!step.Skipped)
            {
                return false;
            }
        }

        return false;
    }

    private int GetHqPercent()
    {
        if (this.Quality == 0)
        {
            return 1;
        }

        var qualityPercent = (int)(Math.Min((double)this.Quality / this.Recipe.MaxQuality, 1) * 100);

        if (qualityPercent >= 100)
        {
            return 100;
        }

        return Tables.HQ_TABLE[qualityPercent];
    }
}