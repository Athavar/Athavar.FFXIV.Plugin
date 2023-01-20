// <copyright file="FocusedSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal class FocusedSynthesis : ProgressAction
{
    private static readonly uint[] IdsValue = { 100235, 100236, 100237, 100238, 100239, 100240, 100241, 100242 };

    /// <inheritdoc />
    public override int Level => 67;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 5;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => simulation.HasComboAvaiable<Observe>() ? 100 : 50;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 200;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}