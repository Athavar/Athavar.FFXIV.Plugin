// <copyright file="QuickInnovation.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Buff;

internal sealed class QuickInnovation : BuffAction
{
    private static readonly uint[] IdsValue = [100459, 100460, 100461, 100462, 100463, 100464, 100465, 100466];

    /// <inheritdoc/>
    public override int Level => 96;

    /// <inheritdoc/>
    public override CraftingClass Class => CraftingClass.ALC;

    /// <inheritdoc/>
    protected override uint[] Ids => IdsValue;

    /// <inheritdoc/>
    public override int GetBaseCPCost(Simulation simulation) => 0;

    /// <inheritdoc/>
    public override int GetDuration(Simulation simulation) => 1;

    /// <inheritdoc/>
    public override Buffs GetBuff() => Buffs.INNOVATION;

    /// <inheritdoc/>
    public override int GetInitialStacks() => 0;

    /// <inheritdoc/>
    public override bool IsSkipsBuffTicks() => true;

    /// <inheritdoc/>
    protected override bool CanBeClipped() => true;

    /// <inheritdoc/>
    protected override OnTick? GetOnTick() => null;

    /// <inheritdoc/>
    protected override bool BaseCanBeUsed(Simulation simulation)
    {
        if (!simulation.CurrentStats.Specialist || simulation.Steps.Any(s => s.Skill.Action is QuickInnovation))
        {
            return false;
        }

        return true;
    }
}