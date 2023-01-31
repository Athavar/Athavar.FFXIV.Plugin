// <copyright file="PreciseTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal class PreciseTouch : QualityAction
{
    private static readonly uint[] IdsValue = { 100128, 100129, 100130, 100131, 100132, 100133, 100134, 100135 };

    /// <inheritdoc />
    public override int Level => 53;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

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

        if (simulation.Safe && !simulation.HasBuff(Buffs.HEART_AND_SOUL))
        {
            return false;
        }

        return simulation.HasBuff(Buffs.HEART_AND_SOUL) || simulation.State is StepState.GOOD or StepState.EXCELLENT;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 150;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}