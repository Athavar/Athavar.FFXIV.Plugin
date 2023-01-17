// <copyright file="PrudentSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

internal class PrudentSynthesis : ProgressAction
{
    private static readonly int[] IdsValue = { 100427, 100428, 100429, 100430, 100431, 100432, 100433, 100434 };

    /// <inheritdoc />
    public override int Level => 88;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => simulation.HasBuff(Buffs.WASTE_NOT) || simulation.HasBuff(Buffs.WASTE_NOT_II) ? SimulationFailCause.INVALID_ACTION : null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 180;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 5;
}