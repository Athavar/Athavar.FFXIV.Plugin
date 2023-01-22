// <copyright file="PrudentTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;

internal class PrudentTouch : QualityAction
{
    private static readonly uint[] IdsValue = { 100227, 100228, 100229, 100230, 100231, 100232, 100233, 100234 };

    /// <inheritdoc />
    public override int Level => 66;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 25;

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
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 5;
}