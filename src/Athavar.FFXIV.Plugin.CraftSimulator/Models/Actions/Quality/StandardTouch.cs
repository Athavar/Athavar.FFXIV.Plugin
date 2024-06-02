// <copyright file="StandardTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal sealed class StandardTouch : QualityAction
{
    private static readonly uint[] IdsValue = [100004, 100018, 100034, 100078, 100048, 100064, 100093, 100109];

    /// <inheritdoc/>
    public override int Level => 18;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => this.HasCombo(simulation) ? 18 : 32;

    /// <inheritdoc/>
    public override bool HasCombo(Simulation simulation) => simulation.HasComboAvaiable<BasicTouch>();

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 125;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}