// <copyright file="IntensiveSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

internal class IntensiveSynthesis : ProgressAction
{
    private static readonly int[] IdsValue = { 100315, 100316, 100317, 100318, 100319, 100320, 100321, 100322 };

    /// <inheritdoc />
    public override int Level => 78;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 6;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Linear)
        {
            return null;
        }

        if (simulation.Safe && !simulation.HasBuff(Buffs.HEART_AND_SOUL))
        {
            return SimulationFailCause.UNSAFE_ACTION;
        }

        if (simulation.HasBuff(Buffs.HEART_AND_SOUL) || simulation.State is StepState.GOOD or StepState.EXCELLENT)
        {
            return null;
        }

        return SimulationFailCause.INVALID_ACTION;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 400;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}