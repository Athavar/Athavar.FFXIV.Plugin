// <copyright file="GeneralAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;

internal abstract class GeneralAction : CraftingAction
{
    public override int GetDurabilityCost(Simulation simulation)
    {
        var divider = 1.0;
        if (simulation.HasBuff(Buffs.WASTE_NOT) || simulation.HasBuff(Buffs.WASTE_NOT_II))
        {
            divider *= 2;
        }

        if (simulation.State == StepState.STURDY)
        {
            divider *= 2;
        }

        return (int)Math.Ceiling(this.GetBaseDurabilityCost(simulation) / divider);
    }

    public double GetBaseBonus(Simulation simulation) => 1;

    public double GetBaseCondition(Simulation simulation) => 1;

    protected abstract int GetPotency(Simulation simulation);

    protected abstract int GetBaseDurabilityCost(Simulation simulation);
}