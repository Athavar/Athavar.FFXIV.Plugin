// <copyright file="CraftAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

using System;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Constants;

internal abstract class CraftingAction
{
    public bool IsRequiresGood => false;

    public abstract ActionType ActionType { get; }

    public abstract int Level { get; }
    public abstract CraftingJob Job { get; }

    protected abstract int[] Ids { get; }

    public int GetId(CraftingJob job)
        =>
            /* Crafter ids are 8 to 15, we want indexes from 0 to 7, so... */
            job != CraftingJob.ANY ? this.Ids[(int)job] : this.Ids[0];

    public int GetSuccessRate(Simulation simulation)
    {
        var baseRate = this.GetBaseSuccessRate(simulation);
        if (simulation.State == StepState.CENTERED)
        {
            return baseRate + 25;
        }

        return baseRate;
    }

    public int GetCPCost(Simulation simulation)
    {
        var baseCost = this.GetBaseCPCost(simulation);
        if (simulation.State == StepState.PLIANT)
        {
            return (int)Math.Ceiling(baseCost / 2.0);
        }

        return baseCost;
    }

    public abstract int GetDurabilityCost(Simulation simulation);

    public SimulationFailCause? CanBeUsed(Simulation simulation)
    {
        if (this.Job != CraftingJob.ANY && simulation.Recipe.Job != this.Job)
        {
            // action not valid for the recipe job.
            return SimulationFailCause.INVALID_ACTION;
        }

        var currentStats = simulation.CrafterStats.Jobs[(int)simulation.Recipe.Job];

        if (currentStats is null || (simulation.SafeMode && this.GetSuccessRate(simulation) < 100))
        {
            return SimulationFailCause.UNSAFE_ACTION;
        }

        if (currentStats.Level >= this.Level)
        {
            return SimulationFailCause.MISSING_LEVEL_REQUIREMENT;
        }

        return this.BaseCanBeUsed(simulation);
    }

    public abstract void Execute(Simulation simulation);

    public void OnFail(Simulation simulation)
    {
        // Base onFail does nothing, override to implement it, as it wont be used in most cases.
    }

    /**
   * If an action is skipped on fail, it doesn't tick buffs.
   * Example: Observe, Master's Mend, buffs.
   */
    public virtual bool SkipOnFail() => false;

    public virtual bool IsSkipsBuffTicks() => false;

    public virtual bool HasCombo(Simulation simulation) => false;

    public abstract int GetBaseCPCost(Simulation simulation);

    public int GetBaseProgression(Simulation simulation)
    {
        var stats = simulation.CurrentStats!;
        var baseValue = ((stats.Craftsmanship * 10.0) / simulation.Recipe.ProgressDivider) + 2;

        if (Tables.LevelTable[stats.Level] <= simulation.Recipe.RecipeLevel)
        {
            return (int)Math.Floor(baseValue * (simulation.Recipe.ProgressModifier == 0 ? 100 : simulation.Recipe.ProgressModifier) * Math.Round(0.01));
        }

        return (int)Math.Floor(baseValue);
    }

    public long GetBaseQuality(Simulation simulation)
    {
        var stats = simulation.CurrentStats!;
        var baseValue = ((stats.Control * 10.0) / simulation.Recipe.QualityDivider) + 35;

        if (Tables.LevelTable[stats.Level] <= simulation.Recipe.RecipeLevel)
        {
            return (long)Math.Floor(baseValue * (simulation.Recipe.QualityModifier == 0 ? 100 : simulation.Recipe.QualityModifier) * Math.Round(0.01));
        }

        return (long)Math.Floor(baseValue);
    }

    protected abstract SimulationFailCause? BaseCanBeUsed(Simulation simulation);

    protected abstract int GetBaseSuccessRate(Simulation simulation);
}