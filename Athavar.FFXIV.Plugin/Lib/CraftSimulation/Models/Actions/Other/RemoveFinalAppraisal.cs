// <copyright file="RemoveFinalAppraisal.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal class RemoveFinalAppraisal : CraftingAction
{
    private static readonly int[] IdsValue = new int[1] { -1 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 42;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override void Execute(Simulation simulation) => simulation.RemoveBuff(Buffs.FINAL_APPRAISAL);

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => simulation.HasBuff(Buffs.FINAL_APPRAISAL) ? null : SimulationFailCause.INVALID_ACTION;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}