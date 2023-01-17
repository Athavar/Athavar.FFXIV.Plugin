// <copyright file="PrudentTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class PrudentTouch : QualityAction
{
    private static readonly int[] IdsValue = { 100227, 100228, 100229, 100230, 100231, 100232, 100233, 100234 };

    /// <inheritdoc />
    public override int Level => 66;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 25;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => simulation.HasBuff(Buffs.WASTE_NOT) || simulation.HasBuff(Buffs.WASTE_NOT_II) ? SimulationFailCause.INVALID_ACTION : null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 5;
}