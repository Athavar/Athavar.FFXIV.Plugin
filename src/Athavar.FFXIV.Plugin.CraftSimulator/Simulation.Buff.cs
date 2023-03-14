// <copyright file="Simulation.Buff.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;

/// <summary>
/// </summary>
public sealed partial class Simulation
{
    private readonly List<EffectiveBuff> effectiveBuffs = new();

    /// <summary>
    ///     Checks if a buff is applied.
    /// </summary>
    /// <param name="buff">The buff category.</param>
    /// <returns>returns a value indication if the buff is applied.</returns>
    public bool HasBuff(Buffs? buff) => this.effectiveBuffs.Find(row => row.Buffs == buff) != null;

    /// <summary>
    ///     Gets the crafting buff.
    /// </summary>
    /// <param name="buff">The buff category.</param>
    /// <returns>returns the buff.</returns>
    public EffectiveBuff? GetBuff(Buffs buff) => this.effectiveBuffs.Find(row => row.Buffs == buff);

    /// <summary>
    ///     Remove a crafting buff.
    /// </summary>
    /// <param name="buff">The buff category.</param>
    public void RemoveBuff(Buffs buff)
    {
        var toRemove = this.effectiveBuffs.Find(row => row.Buffs == buff);
        if (toRemove is not null)
        {
            this.effectiveBuffs.Remove(toRemove);
        }
    }

    /// <summary>
    ///     Add crafting buff.
    /// </summary>
    /// <param name="buff">The buff.</param>
    public void AddBuff(EffectiveBuff buff) => this.effectiveBuffs.Add(buff);

    /// <summary>
    ///     Add Inner Quiet Stacks.
    /// </summary>
    /// <param name="stacks">The count of stacks to add.</param>
    public void AddInnerQuietStacks(int stacks)
    {
        if (!this.HasBuff(Buffs.INNER_QUIET))
        {
            this.effectiveBuffs.Add(
                new EffectiveBuff(int.MaxValue, Math.Min(stacks, 10), Buffs.INNER_QUIET, this.Steps.Count));
        }
        else
        {
            var iq = this.GetBuff(Buffs.INNER_QUIET);
            if (iq != null)
            {
                iq.Stacks = Math.Min(iq.Stacks + stacks, 10);
            }
        }
    }
}