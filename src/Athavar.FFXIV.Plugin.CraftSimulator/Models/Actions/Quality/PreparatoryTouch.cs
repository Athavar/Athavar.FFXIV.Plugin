// <copyright file="PreparatoryTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal class PreparatoryTouch : QualityAction
{
    private static readonly uint[] IdsValue = { 100299, 100300, 100301, 100302, 100303, 100304, 100305, 100306 };

    /// <inheritdoc />
    public override int Level => 71;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 40;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
        base.Execute(simulation);
        simulation.AddInnerQuietStacks(1);
    }

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc />
    protected override int GetPotency(Simulation simulation) => 200;

    /// <inheritdoc />
    protected override int GetBaseDurabilityCost(Simulation simulation) => 20;
}