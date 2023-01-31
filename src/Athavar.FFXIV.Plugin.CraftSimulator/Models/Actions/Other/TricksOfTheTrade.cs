// <copyright file="TricksOfTheTrade.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal class TricksOfTheTrade : CraftingAction
{
    private static readonly uint[] IdsValue = { 100371, 100372, 100373, 100374, 100375, 100376, 100377, 100378 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.CpRecovery;

    /// <inheritdoc />
    public override int Level => 13;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

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
                simulation.AvailableCP = (int)stats.CP;
            }
        }
    }

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && simulation.State is not (StepState.GOOD or StepState.EXCELLENT))
        {
            return SimulationFailCause.INVALID_ACTION;
        }

        return superCause;
    }

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Linear)
        {
            return true;
        }

        if (simulation.Safe)
        {
            return false;
        }

        return simulation.State is (StepState.GOOD or StepState.EXCELLENT);
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}