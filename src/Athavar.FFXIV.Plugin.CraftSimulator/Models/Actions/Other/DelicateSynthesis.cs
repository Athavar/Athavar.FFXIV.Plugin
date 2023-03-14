// <copyright file="DelicateSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal sealed class DelicateSynthesis : GeneralAction
{
    private static readonly uint[] IdsValue = { 100323, 100324, 100325, 100326, 100327, 100328, 100329, 100330 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 76;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        var progressionIncrease = this.GetBaseProgression(simulation);
        var potency = this.GetPotency(simulation);
        var buffMod = this.GetBaseBonus(simulation);
        var conditionMod = this.GetBaseCondition(simulation);

        switch (simulation.State)
        {
            case StepState.MALLEABLE:
                conditionMod *= 1.5;
                break;
        }

        if (simulation.HasBuff(Buffs.MUSCLE_MEMORY))
        {
            buffMod += 1;
            simulation.RemoveBuff(Buffs.MUSCLE_MEMORY);
        }

        if (simulation.HasBuff(Buffs.VENERATION))
        {
            buffMod += 0.5;
        }

        var progressEfficiency = potency * buffMod;
        simulation.Progression += (int)Math.Floor((progressionIncrease * conditionMod * progressEfficiency) / 100.0);

        if (simulation.HasBuff(Buffs.FINAL_APPRAISAL) &&
            simulation.Progression >= simulation.Recipe.Progress)
        {
            simulation.Progression = (int)Math.Min(simulation.Progression, simulation.Recipe.Progress - 1);
            simulation.RemoveBuff(Buffs.FINAL_APPRAISAL);
        }

        // Quality
        this.ExecuteQuality(simulation);
    }

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 32;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;

    private void ExecuteQuality(Simulation simulation)
    {
        var buffMod = this.GetBaseBonus(simulation);
        var conditionMod = this.GetBaseCondition(simulation);
        var potency = this.GetPotency(simulation);
        var qualityIncrease = this.GetBaseQuality(simulation);

        switch (simulation.State)
        {
            case StepState.EXCELLENT:
                conditionMod *= 4;
                break;
            case StepState.POOR:
                conditionMod *= 0.5;
                break;
            case StepState.GOOD:
                conditionMod *= 1.5;
                break;
        }

        buffMod += (simulation.GetBuff(Buffs.INNER_QUIET)?.Stacks ?? 0) / 10.0;
        var buffMult = 1.0;
        if (simulation.HasBuff(Buffs.GREAT_STRIDES))
        {
            buffMult += 1;
            simulation.HasBuff(Buffs.GREAT_STRIDES);
        }

        if (simulation.HasBuff(Buffs.INNOVATION))
        {
            buffMult += 0.5;
        }

        buffMod = Math.Round(buffMod) * Math.Round(buffMult);

        var efficiency = Math.Round(potency * buffMod);

        simulation.Quality += (long)Math.Floor((qualityIncrease * conditionMod * efficiency) / 100);
        simulation.AddInnerQuietStacks(1);
    }
}