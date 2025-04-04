// <copyright file="ByregotsBlessing.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal sealed class ByregotsBlessing : QualityAction
{
    private static readonly uint[] IdsValue = [100339, 100340, 100341, 100342, 100343, 100344, 100345, 100346];

    /// <inheritdoc/>
    public override int Level => 50;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 24;

    /// <inheritdoc/>
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.RemoveBuff(Buffs.INNER_QUIET);
    }

    /// <inheritdoc/>
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && !simulation.HasBuff(Buffs.INNER_QUIET))
        {
            return SimulationFailCause.NO_INNER_QUIET;
        }

        return superCause;
    }

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => simulation.HasBuff(Buffs.INNER_QUIET) && simulation.GetBuff(Buffs.INNER_QUIET)?.Stacks > 0 ? true : false;

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => Math.Min(100 + ((simulation.GetBuff(Buffs.INNER_QUIET)?.Stacks ?? 0) * 20), 300);

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}