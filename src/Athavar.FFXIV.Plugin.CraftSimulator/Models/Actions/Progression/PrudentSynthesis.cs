// <copyright file="PrudentSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Progression;

internal sealed class PrudentSynthesis : ProgressAction
{
    private static readonly uint[] IdsValue = { 100427, 100428, 100429, 100430, 100431, 100432, 100433, 100434 };

    /// <inheritdoc />
    public override int Level => 88;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && !this.BaseCanBeUsed(simulation))
        {
            return SimulationFailCause.INVALID_ACTION;
        }

        return superCause;
    }

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => !simulation.HasBuff(Buffs.WASTE_NOT) && !simulation.HasBuff(Buffs.WASTE_NOT_II);

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 180;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 5;
}