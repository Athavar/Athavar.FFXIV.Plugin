// <copyright file="CarefulObservation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;

internal class CarefulObservation : CraftingAction
{
    private static readonly int[] IdsValue = { 100395, 100396, 100397, 100398, 100399, 100400, 100401, 100402 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 55;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
    }

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override bool IsSkipsBuffTicks() => true;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation) => !simulation.CurrentStats?.Specialist ?? false ? SimulationFailCause.NOT_SPECIALIST : null;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}