// <copyright file="ProgressAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;

internal abstract class ProgressAction : GeneralAction
{
    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Progression;

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
    }
}