// <copyright file="QualityAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;

internal abstract class QualityAction : GeneralAction
{
    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Quality;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
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

        buffMod *= buffMult;

        var efficiency = potency * buffMod;

        simulation.Quality += (long)Math.Floor((qualityIncrease * conditionMod * efficiency) / 100);
        simulation.AddInnerQuietStacks(1);
    }
}