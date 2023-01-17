// <copyright file="Reflect.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

using System.Linq;

internal class Reflect : QualityAction
{
    private static readonly int[] IdsValue = { 100387, 100388, 100389, 100390, 100391, 100392, 100393, 100394 };

    /// <inheritdoc />
    public override int Level => 69;

    /// <inheritdoc />
    public override CraftingJob Job => CraftingJob.ANY;

    /// <inheritdoc />
    protected override int[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 6;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.AddInnerQuietStacks(1);
    }

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    protected override SimulationFailCause? BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Steps.All(s => s.Action.IsSkipsBuffTicks()))
        {
            return null;
        }

        return SimulationFailCause.INVALID_ACTION;
    }

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}