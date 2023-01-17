// <copyright file="Groundwork.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

internal class Groundwork : ProgressAction
{
    private static readonly int[] IdsValue = { 100403, 100404, 100405, 100406, 100407, 100408, 100409, 100410 };

    /// <inheritdoc />
    public override int Level => 72;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation)
    {
        var basePotency = simulation.CurrentStats?.Level >= 86 ? 360 : 300;
        if (simulation.Durability >= this.GetBaseDurabilityCost(simulation))
        {
            return basePotency;
        }

        return basePotency / 2;
    }

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 20;
}