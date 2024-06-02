// <copyright file="IntensiveSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Progression;

internal sealed class IntensiveSynthesis : ProgressAction
{
    private static readonly uint[] IdsValue = [100315, 100316, 100317, 100318, 100319, 100320, 100321, 100322];

    /// <inheritdoc/>
    public override int Level => 78;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 6;

    /// <inheritdoc/>
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && simulation.State is not (StepState.GOOD or StepState.EXCELLENT))
        {
            return SimulationFailCause.INVALID_ACTION;
        }

        return superCause;
    }

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Linear)
        {
            return true;
        }

        if (simulation.Safe && !simulation.HasBuff(Buffs.HEART_AND_SOUL))
        {
            return false;
        }

        return simulation.HasBuff(Buffs.HEART_AND_SOUL) || simulation.State is (StepState.GOOD or StepState.EXCELLENT);
    }

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 400;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}