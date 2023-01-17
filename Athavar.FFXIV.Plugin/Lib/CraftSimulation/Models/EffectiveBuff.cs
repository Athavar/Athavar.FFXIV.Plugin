// <copyright file="EffectiveBuff.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

internal record EffectiveBuff(int MaxDuration, int InitStacks, Buffs Buffs, int AppliedStep, BuffAction.OnTick? TickAction = null, BuffAction.OnExpire? ExpireAction = null)
{
    public int Duration { get; set; } = MaxDuration;

    public int Stacks { get; set; } = InitStacks;
}