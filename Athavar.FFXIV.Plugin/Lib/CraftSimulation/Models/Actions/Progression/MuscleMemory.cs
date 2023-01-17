// <copyright file="MuscleMemory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

using System.Linq;

internal class MuscleMemory : ProgressAction
{
    private static readonly int[] IdsValue = { 100379, 100380, 100381, 100382, 100383, 100384, 100385, 100386 };

    /// <inheritdoc />
    public override int Level => 54;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 6;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 10;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.AddBuff(new EffectiveBuff(simulation.State == StepState.PRIMED ? 7 : 5, 0, Buffs.MUSCLE_MEMORY, simulation.Steps.Count));
    }

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Steps.All(s => s.Action.IsSkipsBuffTicks()))
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