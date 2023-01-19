// <copyright file="TrainedFinesse.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class TrainedFinesse : QualityAction
{
    private static readonly uint[] IdsValue = { 100435, 100436, 100437, 100438, 100439, 100440, 100441, 100442 };

    /// <inheritdoc />
    public override int Level => 90;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 32;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.GetBuff(Buffs.INNER_QUIET)?.Stacks == 10)
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 0;
}