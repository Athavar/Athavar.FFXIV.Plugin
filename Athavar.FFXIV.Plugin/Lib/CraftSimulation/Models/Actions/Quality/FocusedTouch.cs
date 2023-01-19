// <copyright file="FocusedTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal class FocusedTouch : QualityAction
{
    private static readonly uint[] IdsValue = { 100243, 100244, 100245, 100246, 100247, 100248, 100249, 100250 };

    /// <inheritdoc />
    public override int Level => 68;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => simulation.HasComboAvaiable<Observe>() ? 100 : 50;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 150;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}