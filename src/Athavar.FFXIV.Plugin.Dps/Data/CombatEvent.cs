// <copyright file="CombatEvent.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using Athavar.FFXIV.Plugin.Common.Definitions;
using Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;
using FFXIVClientStructs.FFXIV.Client.Game;

internal abstract record CombatEvent
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    public uint SequenceId { get; init; }

    public uint ActorId { get; init; }

    public record Action : CombatEvent
    {
        public uint TargetId { get; init; }

        public ActionId ActionId { get; init; }

        public Common.Definitions.Action? Definition { get; init; }

        public ActionEffectEvent[] Effects { get; init; } = Array.Empty<ActionEffectEvent>();
    }

    public record Death : CombatEvent
    {
        public uint SourceId { get; init; }
    }

    public record DeferredEvent : CombatEvent
    {
        public ActionEffectEvent? EffectEvent { get; init; }
    }

    public record StatusEffect : CombatEvent
    {
        public ushort StatusId { get; init; }

        public uint SourceId { get; init; }

        public bool Grain { get; init; }

        public float Duration { get; init; }
    }

    public abstract record ActionEffectEvent
    {
        public string? ActionName { get; set; } = string.Empty;

        public bool IsSourceEntry => (this.Flags2 & 128U) > 0U;

        public ActionEffectType EffectType { get; init; }

        public uint SourceId { get; set; }

        public uint EffectTargetId { get; init; }

        public uint TargetId => this.IsSourceTarget ? this.SourceId : this.EffectTargetId;

        public byte HitSeverity { get; set; }

        public byte Param1 { get; set; }

        public byte Percentage { get; set; }

        public byte Flags2 { get; set; }

        public byte Multiplier { get; set; }

        public ushort Value { get; set; }

        public bool IsSourceTarget => (this.Flags2 & 0x80) != 0;

        public virtual uint Amount => (uint)(this.Value + ((this.Flags2 & 0x40) == 0x40 ? this.Multiplier << 16 : 0));
    }

    public abstract record Heal : ActionEffectEvent
    {
        public uint Overheal { get; set; }
    }

    public abstract record Damage : ActionEffectEvent
    {
        public virtual DamageType DamageType => DamageType.Unknown;
    }

    public record HoT : Heal
    {
        public uint StatusId { get; init; }
    }

    public record DoT : Damage
    {
        public uint StatusId { get; init; }

        public override DamageType DamageType => (DamageType)(this.Param1 & 0xF);
    }

    public record DamageTaken : Damage
    {
        public ActionType ActionType { get; init; }

        public uint ComboAmount => this.Percentage;

        public bool IsCrit => (this.HitSeverity & 1) == 1;

        public bool IsDirectHit => (this.HitSeverity & 2) == 1;

        public DamageElementType DamageElementType => (DamageElementType)(this.Param1 >> 4);

        public override DamageType DamageType => (DamageType)(this.Param1 & 0xF);

        public bool IsParried => this.EffectType == ActionEffectType.ParriedDamage;

        public bool IsBlocked => this.EffectType == ActionEffectType.BlockedDamage;
    }

    public record Healed : Heal
    {
        public uint ActionId { get; init; }

        public ActionType ActionType { get; init; }

        public bool IsCrit => (this.HitSeverity & 1) == 1;
    }
}