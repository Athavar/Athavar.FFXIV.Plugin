// <copyright file="Simulation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Buff;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

public partial class Simulation
{
    private readonly Random random = new((int)DateTime.UtcNow.Ticks);
    private long startingQuality;

    public Simulation(CrafterStats crafterStats, Recipe recipe, int startingQuality = 0)
    {
        this.CrafterStats = crafterStats;
        this.CurrentStats = crafterStats;
        this.Recipe = recipe;
        this.startingQuality = startingQuality;
        this.Reset();
    }

    public void SetHqIngredients((uint ItemId, byte Amount)[] hqIngredients)
    {
        long quality = 0;
        foreach (var ingredient in hqIngredients)
        {
            var ingredientDetails = this.Recipe.Ingredients.FirstOrDefault(i => i.Id == ingredient.ItemId);
            if (ingredientDetails?.Quality != null)
            {
                quality += ingredientDetails.Quality.Value * ingredient.Amount;
            }
        }

        this.startingQuality = quality;
    }

    public void Repair(int amount)
    {
        this.Durability += amount;
        if (this.Durability > this.Recipe.Durability)
        {
            this.Durability = this.Recipe.Durability;
        }
    }

    public SimulationResult Run(int[] skills, bool linear = false) => this.Run(CraftingSkill.Parse(skills), linear);

    public SimulationResult Run(IEnumerable<CraftingSkills> skills, bool linear = false)
    {
        this.Linear = linear;
        SimulationFailCause? simulationFailCause = null;
        this.Reset();
        var currentStats = this.CurrentStats;
        if (currentStats.Level < this.Recipe.Level)
        {
            simulationFailCause = SimulationFailCause.MISSING_LEVEL_REQUIREMENT;
        }
        else if ((this.Recipe.CraftsmanshipReq is not null && currentStats.Craftsmanship < this.Recipe.CraftsmanshipReq) ||
                 (this.Recipe.ControlReq is not null && currentStats.Control < this.Recipe.ControlReq))
        {
            simulationFailCause = SimulationFailCause.MISSING_STATS_REQUIREMENT;
        }
        else
        {
            foreach (var skill in skills)
            {
                var craftingSkill = CraftingSkill.FindAction(skill);
                var action = craftingSkill.Action;
                ActionResult result;

                SimulationFailCause? failCause = null;
                var canUseAction = action.CanBeUsed(this);
                if (!canUseAction)
                {
                    failCause = action.GetFailCause(this);
                }

                var hasEnoughCp = action.GetBaseCPCost(this) <= this.AvailableCP;
                if (!hasEnoughCp)
                {
                    failCause = SimulationFailCause.NOT_ENOUGH_CP;
                }

                if (this.Success is null && hasEnoughCp && failCause is null)
                {
                    result = this.RunAction(craftingSkill);
                }
                else
                {
                    result = new ActionResult(
                        craftingSkill,
                        null,
                        0,
                        0,
                        0,
                        true,
                        0,
                        this.State,
                        failCause,
                        false);
                }

                this.Steps.Add(result);
            }
        }

        simulationFailCause ??= this.Steps.FirstOrDefault(step => step.FailCause != null)?.FailCause ?? null;

        var res = new SimulationResult(
            this.Steps,
            this.GetHqPercent(),
            this)
        {
            Success = this.Progression >= this.Recipe.Progress,
        };

        if (res.Success && this.Recipe.QualityReq is not null)
        {
            var qualityRequirementMet = this.Quality >= this.Recipe.QualityReq;
            res.Success &= qualityRequirementMet;
            if (!res.Success)
            {
                simulationFailCause = SimulationFailCause.QUALITY_TOO_LOW;
            }
        }

        res.FailCause = simulationFailCause;

        return res;
    }

    public (uint Control, uint Craftsmanship, uint Cp, bool Found) GetMinStats(CraftingSkills[] rotation)
    {
        var totalIterations = 0;
        var originalHqPercent = this.Run(rotation, true).HqPercent;
        var originalStats = this.CurrentStats;
        var res = (
            this.CurrentStats.Control,
            this.CurrentStats.Craftsmanship,
            this.CurrentStats.CP,
            Found: true);

        this.CurrentStats.Craftsmanship = 1;

        var result = this.Run(rotation, true);

        // Three loops, one per stat
        while (!result.Success && totalIterations < 10000)
        {
            this.CurrentStats.Craftsmanship++;
            result = this.Run(rotation, true);
            totalIterations++;
        }

        res.Craftsmanship = this.CurrentStats.Craftsmanship;

        this.CurrentStats.Control = 1;
        result = this.Run(rotation, true);

        while (result.HqPercent < originalHqPercent && totalIterations < 10000)
        {
            this.CurrentStats.Control++;
            result = this.Run(rotation, true);
            totalIterations++;
        }

        res.Control = this.CurrentStats.Control;

        this.CurrentStats.CP = 180;
        result = this.Run(rotation, true);

        while (totalIterations < 10000 && (!result.Success || result.HqPercent < originalHqPercent))
        {
            this.CurrentStats.CP++;
            result = this.Run(rotation, true);
            totalIterations++;
        }

        res.CP = this.CurrentStats.CP;

        if (totalIterations >= 10000)
        {
            res.Found = false;
        }

        this.CurrentStats.CP = originalStats.CP;
        this.CurrentStats.Craftsmanship = originalStats.Craftsmanship;
        this.CurrentStats.Control = originalStats.Control;
        return res;
    }

    public SimulationReliabilityReport GetReliabilityReport(CraftingSkills[] rotation)
    {
        const int simulationTimes = 200;
        var results = new SimulationResult[simulationTimes];
        for (var i = 0; i < simulationTimes; i++)
        {
            results[i] = this.Run(rotation);
        }

        var successPercent = (results.Count(res => res.Success) / results.Length) * 100;
        var hqPercent = results.Sum(sr => sr.HqPercent) / results.Length;
        var hqMedian = 0;
        Array.Sort(results, (a, b) => a.HqPercent - b.HqPercent);
        if (results.Length % 2 == 0)
        {
            hqMedian = results[(int)Math.Floor((double)results.Length / 2)].HqPercent;
        }
        else
        {
            hqMedian =
                (results[(int)Math.Floor((double)results.Length / 2)].HqPercent +
                 results[(int)Math.Ceiling((double)results.Length / 2)].HqPercent) /
                2;
        }

        return new SimulationReliabilityReport(results, successPercent, hqPercent, hqMedian, results[0].HqPercent, results[^1].HqPercent);
    }

    /**
   * Changes the state of the craft,
   * source: https://github.com/Ermad/ffxiv-craft-opt-web/blob/master/app/js/ffxivcraftmodel.js#L255
   */
    private void TickState()
    {
        // If current state is EXCELLENT, then next one is poor
        if (this.State == StepState.EXCELLENT)
        {
            this.State = StepState.POOR;
            return;
        }

        var currentStats = this.CurrentStats;
        if (currentStats is null)
        {
            return;
        }

        // LV 63 Trait for improved Good chances (Quality Assurance)
        var goodChance = currentStats.Level >= 63 ? 0.25 : 0.2;

        var statesAndRates = this.Recipe.PossibleConditions
           .Where(condition => condition != StepState.NORMAL)
           .Select(condition =>
            {
                // Default rate - most conditions are 12% so here we are.
                var rate = 0.12;
                switch (condition)
                {
                    case StepState.GOOD:
                        rate = this.Recipe.Expert ? 0.12 : goodChance;
                        break;
                    case StepState.EXCELLENT:
                        rate = this.Recipe.Expert ? 0 : 0.04;
                        break;
                    case StepState.POOR:
                        rate = 0;
                        break;
                    case StepState.CENTERED:
                        rate = 0.15;
                        break;
                    case StepState.PLIANT:
                        rate = 0.12;
                        break;
                    case StepState.STURDY:
                        rate = 0.15;
                        break;
                    case StepState.MALLEABLE:
                        rate = 0.12;
                        break;
                    case StepState.PRIMED:
                        rate = 0.12;
                        break;
                }

                return (
                    item: condition,
                    weight: rate);
            }).ToList();

        var nonNormalRate = statesAndRates
           .Select(val => val.weight)
           .Sum();

        statesAndRates.Add((StepState.NORMAL, 1 - nonNormalRate));

        var threshold = this.random.NextDouble();

        var check = 0.0;
        foreach (var (item, weight) in statesAndRates)
        {
            check += weight;
            if (check > threshold)
            {
                this.State = item;
                return;
            }
        }

        this.State = statesAndRates.Last().item;
    }

    private ActionResult RunAction(CraftingSkill skill)
    {
        var action = skill.Action;

        // The roll for the current action's success rate, 0 if ideal mode, as 0 will even match a 1% chances.
        var probabilityRoll = this.Linear ? 0 : this.random.Next(0, 100);

        var success = false;
        var qualityBefore = this.Quality;
        var progressionBefore = this.Progression;
        var durabilityBefore = this.Durability;
        var cpBefore = this.AvailableCP;
        SimulationFailCause? failCause = null;
        var combo = action.HasCombo(this);

        if (this.SafeMode &&
            (action.GetSuccessRate(this) < 100 ||
             (action.IsRequiresGood && !this.HasBuff(Buffs.HEART_AND_SOUL))))
        {
            failCause = SimulationFailCause.UNSAFE_ACTION;
            this.Safe = false;
        }
        else
        {
            if (action.GetSuccessRate(this) >= probabilityRoll)
            {
                action.Execute(this);
                success = true;
            }
            else
            {
                action.OnFail(this);
            }
        }

        // Even if the action failed, we have to remove the durability cost
        this.Durability -= action.GetDurabilityCost(this);

        // Even if the action failed, CP has to be consumed too
        this.AvailableCP -= action.GetCPCost(this);

        if (this.Progression >= this.Recipe.Progress)
        {
            this.Success = true;
        }
        else if (this.Durability <= 0)
        {
            // Check durability to see if the craft is failed or not
            this.Success = false;
            failCause = SimulationFailCause.DURABILITY_REACHED_ZERO;
        }

        var skipTicksOnFail = this.Success == false && action.SkipOnFail();
        if (this.Success is null && !action.IsSkipsBuffTicks() && !skipTicksOnFail)
        {
            this.TickBuffs(action);
        }

        if (!this.Linear && action is not FinalAppraisal or RemoveFinalAppraisal)
        {
            this.TickState();
        }

        return new ActionResult(
            skill,
            success,
            this.Quality - qualityBefore,
            this.Progression - progressionBefore,
            this.AvailableCP - cpBefore,
            false,
            this.Durability - durabilityBefore,
            this.State,
            failCause,
            combo);
    }

    private void TickBuffs(CraftingAction action)
    {
        foreach (var effectiveBuff in this.effectiveBuffs)
        {
            // We are checking the appliedStep because ticks only happen at the beginning of the second turn after the application,
            // For instance, Great strides launched at turn 1 will start to loose duration at the beginning of turn 3
            if (effectiveBuff.AppliedStep < this.Steps.Count)
            {
                // If the buff has something to do, let it do it
                effectiveBuff.TickAction?.Invoke(this, action);

                effectiveBuff.Duration--;
            }
        }

        foreach (var effectiveBuff in this.effectiveBuffs
           .Where(buff => buff.Duration <= 0))
        {
            effectiveBuff.ExpireAction?.Invoke(this);
        }

        this.effectiveBuffs.RemoveAll(buff => buff.Duration <= 0);
    }
}