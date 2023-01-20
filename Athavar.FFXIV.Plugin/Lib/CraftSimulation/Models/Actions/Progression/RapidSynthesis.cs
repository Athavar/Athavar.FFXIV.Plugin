// <copyright file="RapidSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;

internal class RapidSynthesis : ProgressAction
{
    private static readonly uint[] IdsValue = { 100363, 100364, 100365, 100366, 100367, 100368, 100369, 100370 };

    /// <inheritdoc />
    public override int Level => 9;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 50;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation)
    {
        if (simulation.CurrentStats?.Level >= 63)
        {
            // Rapid Synthesis Mastery
            return 500;
        }

        return 250;
    }

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}