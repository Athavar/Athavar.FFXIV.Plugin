// <copyright file="Manipulation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;

internal class Manipulation : BuffAction
{
    private static readonly uint[] IdsValue = { 4574, 4575, 4576, 4577, 4578, 4579, 4580, 4581 };

    /// <inheritdoc />
    public override int Level => 65;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Repair;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 96;

    /// <inheritdoc />
    public override int GetDuration(Simulation simulation) => 8;

    /// <inheritdoc />
    public override Buffs GetBuff() => Buffs.MANIPULATION;

    /// <inheritdoc />
    public override int GetInitialStacks() => 0;

    /// <inheritdoc />
    public override int GetWaitDuration() => 2;

    /// <inheritdoc />
    protected override OnTick? GetOnTick() => this.OnTick;

    /// <inheritdoc />
    protected override bool CanBeClipped() => true;

    private new void OnTick(Simulation simulation, CraftingAction action) => simulation.Repair(5);
}