// <copyright file="StatusEffect.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

public class StatusEffect
{
    public uint Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Potency[]? PotencyEffects { get; set; }

    public TimeProc? TimeProc { get; set; }

    public Multiplier[]? Multipliers { get; set; }

    public ReactiveProc? ReactiveProc { get; set; }

    public DamageShield? DamageShield { get; set; }
}