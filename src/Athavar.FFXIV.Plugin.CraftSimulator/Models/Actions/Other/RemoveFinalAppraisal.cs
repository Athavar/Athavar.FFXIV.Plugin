// <copyright file="RemoveFinalAppraisal.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal class RemoveFinalAppraisal : CraftingAction
{
    private static readonly uint[] IdsValue = { 0 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 42;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override void Execute(Simulation simulation) => simulation.RemoveBuff(Buffs.FINAL_APPRAISAL);

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => simulation.HasBuff(Buffs.FINAL_APPRAISAL) ? true : false;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}