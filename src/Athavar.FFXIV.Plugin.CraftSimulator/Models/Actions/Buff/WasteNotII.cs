// <copyright file="WasteNotII.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Buff;

internal sealed class WasteNotII : BuffAction
{
    private static readonly uint[] IdsValue = [4639, 4640, 4641, 4642, 4643, 4644, 19002, 19003];

    /// <inheritdoc/>
    public override int Level => 47;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 98;

    /// <inheritdoc/>
    public override int GetDuration(Simulation simulation) => 8;

    /// <inheritdoc/>
    public override Buffs GetBuff() => Buffs.WASTE_NOT_II;

    /// <inheritdoc/>
    public override int GetInitialStacks() => 0;

    /// <inheritdoc/>
    public override IEnumerable<Buffs> GetOverrides() => base.GetOverrides().Append(Buffs.WASTE_NOT);

    /// <inheritdoc/>
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc/>
    protected override bool CanBeClipped() => true;
}