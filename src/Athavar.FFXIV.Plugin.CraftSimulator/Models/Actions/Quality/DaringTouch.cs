// <copyright file="DaringTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal sealed class DaringTouch : QualityAction
{
    private static readonly uint[] IdsValue = [100451, 100452, 100453, 100454, 100455, 100456, 100457, 100458];

    /// <inheritdoc/>
    public override int Level => 96;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    public override bool HasCombo(Simulation simulation) => simulation.HasBuff(Buffs.EXPEDIENCE);

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => simulation.HasBuff(Buffs.EXPEDIENCE);

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 60;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 150;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}