// <copyright file="MuscleMemory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Progression;

internal sealed class MuscleMemory : ProgressAction
{
    private static readonly uint[] IdsValue = [100379, 100380, 100381, 100382, 100383, 100384, 100385, 100386];

    /// <inheritdoc/>
    public override int Level => 54;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 6;

    /// <inheritdoc/>
    public override int GetDurabilityCost(Simulation simulation) => 10;

    /// <inheritdoc/>
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.AddBuff(new EffectiveBuff(simulation.State == StepState.PRIMED ? 7 : 5, 0, Buffs.MUSCLE_MEMORY, simulation.Steps.Count));
    }

    /// <inheritdoc/>
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && !this.BaseCanBeUsed(simulation))
        {
            return SimulationFailCause.INVALID_ACTION;
        }

        return superCause;
    }

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Steps.All(s => s.Skill.Action.IsSkipsBuffTicks()))
        {
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 300;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}