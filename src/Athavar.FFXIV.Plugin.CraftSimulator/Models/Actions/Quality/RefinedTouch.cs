// <copyright file="RefinedTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal sealed class RefinedTouch : QualityAction
{
    private static readonly uint[] IdsValue = [100443, 100444, 100445, 100446, 100447, 100448, 100449, 100450];

    /// <inheritdoc/>
    public override int Level => 92;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 24;

    /// <inheritdoc/>
    public override void Execute(Simulation simulation)
    {
        var combo = this.HasCombo(simulation);
        base.Execute(simulation);
        if (combo)
        {
            simulation.AddInnerQuietStacks(1);
        }
    }

    /// <inheritdoc/>
    public override bool HasCombo(Simulation simulation) => simulation.HasComboAvaiable<BasicTouch>();

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => throw new NotImplementedException();

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}