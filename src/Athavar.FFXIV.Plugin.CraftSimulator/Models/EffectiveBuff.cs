// <copyright file="EffectiveBuff.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;

public record EffectiveBuff(int MaxDuration, int InitStacks, Buffs Buffs, int AppliedStep, BuffAction.OnTick? TickAction = null, BuffAction.OnExpire? ExpireAction = null)
{
    public int Duration { get; set; } = MaxDuration;

    public int Stacks { get; set; } = InitStacks;
}