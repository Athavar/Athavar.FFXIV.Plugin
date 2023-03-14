// <copyright file="GreatStrides.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Buff;

internal sealed class GreatStrides : BuffAction
{
    private static readonly uint[] IdsValue = { 260, 261, 262, 263, 265, 264, 266, 267 };

    /// <inheritdoc />
    public override int Level => 21;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

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