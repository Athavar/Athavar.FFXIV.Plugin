// <copyright file="CarefulObservation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal sealed class CarefulObservation : CraftingAction
{
    private static readonly uint[] IdsValue = { 100395, 100396, 100397, 100398, 100399, 100400, 100401, 100402 };

    /// <inheritdoc />
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc />
    public override int Level => 55;

    /// <inheritdoc />
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc />
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc />
    public override int GetDurabilityCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override void Execute(Simulation simulation)
    {
    }

    /// <inheritdoc />
    public override SimulationFailCause? GetFailCause(Simulation simulation)
    {
        var superCause = base.GetFailCause(simulation);
        if (superCause is null && !simulation.CurrentStats.Specialist)
        {
            return SimulationFailCause.NOT_SPECIALIST;
        }

        return superCause;
    }

    /// <inheritdoc />
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc />
    public override bool IsSkipsBuffTicks() => true;

    /// <inheritdoc />
    protected override bool BaseCanBeUsed(Simulation simulation) => !simulation.CurrentStats.Specialist ? false : true;

    /// <inheritdoc />
    protected override int GetBaseSuccessRate(Simulation simulation) => 100;
}