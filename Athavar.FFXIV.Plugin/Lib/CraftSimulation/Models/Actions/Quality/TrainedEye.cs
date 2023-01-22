// <copyright file="TrainedEye.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

using System.Linq;

internal class TrainedEye : CraftingAction
{
    private static readonly uint[] IdsValue = { 100283, 100284, 100285, 100286, 100287, 100288, 100289, 100290 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Quality;

    /// <inheritdoc />
    public override int Level => 80;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override void Execute(Simulation simulation) => simulation.Quality = simulation.Recipe.MaxQuality;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 250;

    /// <inheritdoc />
    public override bool SkipOnFail() => true;

    /// <inheritdoc />
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && (simulation.Recipe.Expert || simulation.CurrentStats.Level - simulation.Recipe.Level < 10 || simulation.Steps.Any()))
        {
            return SimulationFailCause.INVALID_ACTION;
        }

        return superCause;
    }

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => !simulation.Recipe.Expert && simulation.CurrentStats.Level - simulation.Recipe.Level >= 10 && !simulation.Steps.Any();

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}