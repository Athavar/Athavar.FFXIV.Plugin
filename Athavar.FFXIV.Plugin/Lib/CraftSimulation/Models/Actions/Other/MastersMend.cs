// <copyright file="MastersMend.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal class MastersMend : CraftingAction
{
    private static readonly uint[] IdsValue = { 100003, 100017, 100032, 100047, 100062, 100077, 100092, 100107 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Repair;

    /// <inheritdoc />
    public override int Level => 7;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override void Execute(Simulation simulation) => simulation.Repair(30);

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 88;

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}