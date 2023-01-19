// <copyright file="GreatStrides.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;

internal class GreatStrides : BuffAction
{
    private static readonly uint[] IdsValue = { 260, 261, 262, 263, 264, 265, 266, 267 };

    /// <inheritdoc />
    public override int Level => 21;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 32;

    /// <inheritdoc />
    public override int GetDuration(Simulation simulation) => 3;

    /// <inheritdoc />
    public override Buffs GetBuff() => Buffs.GREAT_STRIDES;

    /// <inheritdoc />
    public override int GetInitialStacks() => 0;

    /// <inheritdoc />
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc />
    protected override bool CanBeClipped() => true;
}