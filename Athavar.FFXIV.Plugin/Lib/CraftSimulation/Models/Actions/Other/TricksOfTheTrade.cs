// <copyright file="TricksOfTheTrade.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal class TricksOfTheTrade : CraftingAction
{
    private static readonly int[] IdsValue = { 100371, 100372, 100373, 100374, 100375, 100376, 100377, 100378 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.CpRecovery;

    /// <inheritdoc />
    public override int Level => 13;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        if (simulation.HasBuff(Buffs.HEART_AND_SOUL) || simulation.State is StepState.GOOD or StepState.EXCELLENT)
        {
            var stats = simulation.CurrentStats;
            simulation.AvailableCP += 20;
            if (simulation.AvailableCP > stats?.CP)
            {
                simulation.AvailableCP = stats.CP;
            }
        }
    }

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Linear)
        {
            return null;
        }

        if (simulation.Safe)
        {
            return SimulationFailCause.UNSAFE_ACTION;
        }

        if (simulation.State is StepState.GOOD or StepState.EXCELLENT)
        {
            return null;
        }

        return SimulationFailCause.INVALID_ACTION;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}