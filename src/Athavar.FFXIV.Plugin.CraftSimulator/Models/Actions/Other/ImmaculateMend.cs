// <copyright file="ImmaculateMend.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal sealed class ImmaculateMend : CraftingAction
{
    private static readonly uint[] IdsValue = [100467, 100468, 100469, 100470, 100471, 100472, 100473, 100474];

    /// <inheritdoc/>
    public override ActionType ActionType => ActionType.Repair;

    /// <inheritdoc/>
    public override int Level => 98;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc/>
    public override void Execute(Simulation simulation)
    {
        simulation.Durability = simulation.Recipe.Durability;
    }

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 112;

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}