// <copyright file="TrainedPerfection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;

internal sealed class TrainedPerfection : BuffAction
{
    private static readonly uint[] IdsValue = [100475, 100476, 100477, 100478, 100479, 100480, 100481, 100482];

    /// <inheritdoc/>
    public override ActionType ActionType => ActionType.Other;

    /// <inheritdoc/>
    public override int Level => 100;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ANY;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc/>
    public override bool SkipOnFail() => true;

    public override Buffs GetBuff() => Buffs.TRAINED_PERFECTION;

    public override int GetInitialStacks() => 0;

    /// <inheritdoc/>
    public override int GetDuration(Simulation simulation) => int.MaxValue;

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (simulation.Steps.Any(s => s.Skill.Action is TrainedPerfection))
        {
            return false;
        }

        return true;
    }

    protected override OnTick? GetOnTick() => null;
}