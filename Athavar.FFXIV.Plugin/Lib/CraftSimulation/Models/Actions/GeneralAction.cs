// <copyright file="GeneralAction.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

using System;

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