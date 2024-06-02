// <copyright file="WasteNot.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Buff;

internal sealed class WasteNot : BuffAction
{
    private static readonly uint[] IdsValue = [4631, 4632, 4633, 4634, 4635, 4636, 4637, 4638];

    /// <inheritdoc/>
    public override int Level => 15;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 56;

    /// <inheritdoc/>
    public override int GetDuration(Simulation simulation) => 4;

    /// <inheritdoc/>
    public override Buffs GetBuff() => Buffs.WASTE_NOT;

    /// <inheritdoc/>
    public override int GetInitialStacks() => 0;

    /// <inheritdoc/>
    public override IEnumerable<Buffs> GetOverrides() => base.GetOverrides().Append(Buffs.WASTE_NOT_II);

    /// <inheritdoc/>
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc/>
    protected override bool CanBeClipped() => true;
}