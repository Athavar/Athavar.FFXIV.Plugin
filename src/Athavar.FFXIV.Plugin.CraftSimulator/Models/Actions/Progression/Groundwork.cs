// <copyright file="Groundwork.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Progression;

internal class Groundwork : ProgressAction
{
    private static readonly uint[] IdsValue = { 100403, 100404, 100405, 100406, 100407, 100408, 100409, 100410 };

    /// <inheritdoc />
    public override int Level => 72;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 18;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation)
    {
        var basePotency = simulation.CurrentStats?.Level >= 86 ? 360 : 300;
        if (simulation.Durability >= this.GetBaseDurabilityCost(simulation))
        {
            return basePotency;
        }

        return basePotency / 2;
    }

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 20;
}