// <copyright file="Innovation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;

internal class Innovation : BuffAction
{
    private static readonly int[] IdsValue = { 19004, 19005, 19006, 19007, 19008, 19009, 19010, 19011 };

    /// <inheritdoc />
    public override int Level => 26;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    public override int GetDuration(Simulation simulation) => 4;

    /// <inheritdoc />
    public override Buffs GetBuff() => Buffs.INNOVATION;

    /// <inheritdoc />
    public override int GetInitialStacks() => 0;

    /// <inheritdoc />
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc />
    protected override bool CanBeClipped() => true;
}