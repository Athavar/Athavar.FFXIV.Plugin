// <copyright file="TrainedFinesse.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class TrainedFinesse : QualityAction
{
    private static readonly int[] IdsValue = { 100435, 100436, 100437, 100438, 100439, 100440, 100441, 100442 };

    /// <inheritdoc />
    public override int Level => 90;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 32;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.GetBuff(Buffs.INNER_QUIET)?.Stacks == 10)
        {
            return null;
        }

        return SimulationFailCause.INVALID_ACTION;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 0;
}