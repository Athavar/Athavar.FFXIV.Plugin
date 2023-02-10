// <copyright file="CarefulSynthesis.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Progression;

internal class CarefulSynthesis : ProgressAction
{
    private static readonly uint[] IdsValue = { 100203, 100204, 100205, 100206, 100207, 100208, 100209, 100210 };

    /// <inheritdoc />
    public override int Level => 62;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 7;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation)
    {
        if (simulation.CurrentStats?.Level >= 82)
        {
            // Basic Synthesis Mastery
            return 180;
        }

        return 150;
    }

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}