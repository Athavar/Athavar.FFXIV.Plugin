// <copyright file="HeartAndSoul.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class HeartAndSoul : BuffAction
{
    private static readonly uint[] IdsValue = { 100419, 100420, 100421, 100422, 100423, 100424, 100425, 100426 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 86;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override int GetDuration(Simulation simulation) => int.MaxValue;

    /// <inheritdoc />
    public override Buffs GetBuff() => Buffs.HEART_AND_SOUL;

    /// <inheritdoc />
    public override int GetInitialStacks() => 0;

    /// <inheritdoc />
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && !simulation.CurrentStats.Specialist)
        {
            return SimulationFailCause.NOT_SPECIALIST;
        }

        return superCause;
    }

    /// <inheritdoc />
    protected override OnTick? GetOnTick() => this.OnTick;

    /// <inheritdoc />
    protected override bool CanBeClipped() => true;

    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (!simulation.CurrentStats.Specialist)
        {
            return false;
        }

        return base.BaseCanBeUsed(simulation);
    }

    private new void OnTick(Simulation simulation, CraftingAction action)
    {
        var usedOnNonGoodOrExcellent = simulation.State is not StepState.GOOD or StepState.EXCELLENT;

        // If linear, this buff will be removed if last action is one of the buffed ones.
        if (usedOnNonGoodOrExcellent && action is PreciseTouch or IntensiveSynthesis or TricksOfTheTrade)
        {
            simulation.RemoveBuff(this.GetBuff());
        }
    }
}