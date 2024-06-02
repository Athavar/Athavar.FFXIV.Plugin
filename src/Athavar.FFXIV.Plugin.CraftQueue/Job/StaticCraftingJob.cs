// <copyright file="StaticCraftingJob.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Job;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.CraftSimulator;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models;
using Lumina.Excel.GeneratedSheets;

internal sealed class StaticCraftingJob : BaseCraftingJob
{
    private readonly string localizedExcellent;

    private readonly IStaticRotationResolver rotationResolver;
    private readonly Simulation simulation;

    private readonly int hqPercent;

    private SimulationResult simulationResult;

    public StaticCraftingJob(CraftQueue queue, RecipeExtended recipe, IStaticRotationResolver rotationResolver, Gearset gearset, uint count, BuffConfig buffConfig, (uint ItemId, byte Amount)[] hqIngredients, CraftingJobFlags flags)
        : base(queue, recipe, rotationResolver, gearset, count, buffConfig, hqIngredients, flags)
    {
        this.rotationResolver = rotationResolver;
        this.simulation = new(gearset.ToCrafterStats(), this.Recipe)
        {
            CurrentStatModifiers = [buffConfig.Food?.Stats, buffConfig.Potion?.Stats],
        };

        this.localizedExcellent = this.queue.DalamudServices.DataManager.GetExcelSheet<Addon>()?.GetRow(228)?.Text.ToString().ToLowerInvariant() ?? throw new AthavarPluginException();

        this.simulation.SetHqIngredients(hqIngredients);
        this.RunSimulation();
        this.hqPercent = this.simulationResult.HqPercent;
    }

    private IList<ActionResult> Steps => this.simulationResult.Steps;

    /// <inheritdoc/>
    protected override void OnCheckCurrentEquipment(Gearset currentEquipped) => this.simulation.CrafterStats = currentEquipped.ToCrafterStats();

    protected override int? MutateRotation()
    {
        var rotation = this.rotationResolver.Rotation;
        var ci = this.queue.CommandInterface;

        // Byregots fail save
        if (!this.Recipe.Expert && this.RotationCurrentStep + 1 < rotation.Length &&
            rotation[this.RotationCurrentStep + 1] == CraftingSkills.ByregotsBlessing &&
            ci.HasCondition(this.localizedExcellent))
        {
            var failSaveAction = CraftingSkills.TricksOfTheTrade;

            var testStepStates = new StepState?[rotation.Length];
            testStepStates[this.RotationCurrentStep] = StepState.EXCELLENT;
            testStepStates[this.RotationCurrentStep + 1] = StepState.POOR;

            IEnumerable<CraftingSkills> GetTestRotation()
            {
                for (var i = 0; i < rotation.Length; i++)
                {
                    if (i == this.RotationCurrentStep)
                    {
                        yield return failSaveAction;
                    }

                    yield return rotation[i];
                }
            }

            var result = this.simulation.Run(GetTestRotation(), stepStates: testStepStates);
            if (result.Success && ci.UseAction(CraftingSkill.FindAction(failSaveAction).Action.GetId(this.Recipe.Class)))
            {
                return -1000;
            }
        }

        return base.MutateRotation();
    }

    protected override void ActionUseFailed(CraftingSkill skill)
    {
        if (this.RotationCurrentStep >= this.Steps.Count)
        {
            return;
        }

        var step = this.Steps[this.RotationCurrentStep];
        if (step.FailCause == SimulationFailCause.NOT_ENOUGH_CP || (step.Success != true && skill.Action.SkipOnFail()))
        {
            ++this.currentRotationStep;
        }
    }

    protected override BuffApplyTest? CalcMissingBuffs(StatModifiers?[] currentStatModifier)
    {
        this.simulation.CurrentStatModifiers = (StatModifiers?[])currentStatModifier.Clone();

        var buffApplyStats = BuffApplyTest.None;
        for (var i = 0; i < 4; i++)
        {
            buffApplyStats = (BuffApplyTest)i;
            if (this.BuffConfig.Food is { } food && (buffApplyStats & BuffApplyTest.Food) != 0)
            {
                this.simulation.CurrentStatModifiers[0] = food.Stats;
            }

            if (this.BuffConfig.Potion is { } potion && (buffApplyStats & BuffApplyTest.Potion) != 0)
            {
                this.simulation.CurrentStatModifiers[1] = potion.Stats;
            }

            this.RunSimulation();

            if (this.BuffConfig.Food is not null && (buffApplyStats & BuffApplyTest.Food) != 0)
            {
                this.simulation.CurrentStatModifiers[0] = currentStatModifier[0];
            }

            if (this.BuffConfig.Potion is not null && (buffApplyStats & BuffApplyTest.Potion) != 0)
            {
                this.simulation.CurrentStatModifiers[1] = currentStatModifier[1];
            }

            if (this.simulationResult.Success && this.simulationResult.HqPercent >= this.hqPercent)
            {
                break;
            }

            if (buffApplyStats == BuffApplyTest.Both)
            {
                return null;
            }
        }

        return buffApplyStats;
    }

    [MemberNotNull(nameof(simulationResult))]
    private void RunSimulation() => this.simulationResult = this.simulation.Run(this.rotationResolver.Rotation, true);
}