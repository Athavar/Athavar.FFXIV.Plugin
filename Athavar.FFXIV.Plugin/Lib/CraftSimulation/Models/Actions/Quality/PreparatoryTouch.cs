// <copyright file="PreparatoryTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class PreparatoryTouch : QualityAction
{
    private static readonly int[] IdsValue = { 100299, 100300, 100301, 100302, 100303, 100304, 100305, 100306 };

    /// <inheritdoc />
    public override int Level => 71;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 40;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.AddInnerQuietStacks(1);
    }

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 200;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 20;
}