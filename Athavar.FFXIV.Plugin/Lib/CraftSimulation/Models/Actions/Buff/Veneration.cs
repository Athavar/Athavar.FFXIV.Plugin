// <copyright file="Veneration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;

internal class Veneration : BuffAction
{
    private static readonly uint[] IdsValue = { 19297, 19298, 19299, 19300, 19301, 19302, 19303, 19304 };

    /// <inheritdoc />
    public override int Level => 15;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    public override int GetDuration(Simulation simulation) => 4;

    /// <inheritdoc />
    public override Buffs GetBuff() => Buffs.VENERATION;

    /// <inheritdoc />
    public override int GetInitialStacks() => 0;

    /// <inheritdoc />
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc />
    protected override bool CanBeClipped() => true;
}