// <copyright file="Simulation.Buff.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation;

using System;
using System.Collections.Generic;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

/// <summary>
/// </summary>
internal partial class Simulation
{
    private readonly List<EffectiveBuff> effectiveBuffs = new();

    /// <summary>
    ///     Checks if a buff is applied.
    /// </summary>
    /// <param name="buffs">The buff category.</param>
    /// <returns>returns a value indication if the buff is applied.</returns>
    public bool HasBuff(Buffs? buffs) => this.effectiveBuffs.Find(row => row.Buffs == buffs) != null;

    /// <summary>
    ///     Gets the crafting buff.
    /// </summary>
    /// <param name="buffs">The buff category.</param>
    /// <returns>returns the buff.</returns>
    public EffectiveBuff? GetBuff(Buffs buffs) => this.effectiveBuffs.Find(row => row.Buffs == buffs);

    /// <summary>
    ///     Remove a crafting buff.
    /// </summary>
    /// <param name="buffs">The buff category.</param>
    public void RemoveBuff(Buffs buffs)
    {
        var toRemove = this.effectiveBuffs.Find(row => row.Buffs != buffs);
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