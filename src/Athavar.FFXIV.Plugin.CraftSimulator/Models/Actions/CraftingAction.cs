// <copyright file="CraftingAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;

using Athavar.FFXIV.Plugin.CraftSimulator.Constants;

public abstract class CraftingAction
{
    public virtual bool IsDeprecated => false;

    public bool IsRequiresGood => false;

    /// <summary>
    ///     Gets the type of the action.
    /// </summary>
    public abstract ActionType ActionType { get; }

    /// <summary>
    ///     Gets the unlock level of the action.
    /// </summary>
    public abstract int Level { get; }

    public abstract CraftingClass Class { get; }

    protected abstract uint[] Ids { get; }

    public virtual int GetWaitDuration() => this.ActionType == ActionType.Buff ? 2 : 3;

    public uint GetId(CraftingClass @class) => @class != CraftingClass.ANY ? this.Ids[(int)@class] : this.Ids[0];

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

    public bool CanBeUsed(Simulation simulation)
    {
        if (this.Class != CraftingClass.ANY && simulation.Recipe.Class != this.Class)
        {
            // action not valid for the recipe job.
            return false;
        }

        var currentStats = simulation.CurrentStats;

        if (simulation.SafeMode && this.GetSuccessRate(simulation) < 100)
        {
            return false;
        }

        if (currentStats.Level < this.Level)
        {
            return false;
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
            return (int)Math.Floor(baseValue * (simulation.Recipe.ProgressModifier == 0 ? 100 : simulation.Recipe.ProgressModifier) * 0.01);
        }

        return (int)Math.Floor(baseValue);
    }

    public virtual SimulationFailCause? GetFailCause(Simulation simulation)
    {
        if (simulation.Success == true)
        {
            return null;
        }

        if (simulation.SafeMode && this.GetSuccessRate(simulation) < 100)
        {
            return SimulationFailCause.UNSAFE_ACTION;
        }

        if (this.Class != CraftingClass.ANY && simulation.Recipe.Class != this.Class)
        {
            // action not valid for the recipe job.
            return SimulationFailCause.INVALID_ACTION;
        }

        if (simulation.CurrentStats.Level < this.Level)
        {
            return SimulationFailCause.MISSING_LEVEL_REQUIREMENT;
        }

        return null;
    }

    public long GetBaseQuality(Simulation simulation)
    {
        var stats = simulation.CurrentStats!;
        var baseValue = ((stats.Control * 10.0) / simulation.Recipe.QualityDivider) + 35;

        if (Tables.LevelTable[stats.Level] <= simulation.Recipe.RecipeLevel)
        {
            return (long)Math.Floor(baseValue * (simulation.Recipe.QualityModifier == 0 ? 100 : simulation.Recipe.QualityModifier) * 0.01);
        }

        return (long)Math.Floor(baseValue);
    }

    protected abstract bool BaseCanBeUsed(Simulation simulation);

    protected abstract int GetBaseSuccessRate(Simulation simulation);
}