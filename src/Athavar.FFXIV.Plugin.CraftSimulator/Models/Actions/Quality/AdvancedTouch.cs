// <copyright file="AdvancedTouch.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;

internal sealed class AdvancedTouch : QualityAction
{
    private static readonly uint[] IdsValue = [100411, 100412, 100413, 100414, 100415, 100416, 100417, 100418];

    /// <inheritdoc/>
    public override int Level => 84;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => this.HasCombo(simulation) ? 18 : 46;

    public override bool HasCombo(Simulation simulation)
    {
        for (var index = simulation.Steps.Count - 1; index >= 0; index--)
        {
            var step = simulation.Steps[index];
            if (step.Success == true && step.Skill.Action is StandardTouch && step.Combo)
            {
                return true;
            }

            // If there's an action that isn't skipped (fail or not), combo is broken
            if (!step.Skipped)
            {
                return false;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation) => true;

    /// <inheritdoc/>
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;

    /// <inheritdoc/>
    protected override int GetPotency(Simulation simulation) => 150;

    /// <inheritdoc/>
    protected override int GetBaseDurabilityCost(Simulation simulation) => 10;
}