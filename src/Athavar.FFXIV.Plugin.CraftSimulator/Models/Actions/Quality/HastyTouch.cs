// <copyright file="HastyTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal sealed class HastyTouch : QualityAction
{
    private static readonly uint[] IdsValue = [100355, 100356, 100357, 100358, 100359, 100360, 100361, 100362];

    /// <inheritdoc/>
    public override int Level => 9;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 60;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}