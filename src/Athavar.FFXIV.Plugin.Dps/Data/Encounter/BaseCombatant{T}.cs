// <copyright file="BaseCombatant{T}.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

internal abstract class BaseCombatant<T> : BaseCombatant
    where T : BaseCombatant
{
    protected readonly BaseEncounter<T> Encounter;

    protected BaseCombatant(BaseEncounter<T> encounter, ulong gameObjectId, uint dataId)
    {
        this.Encounter = encounter;
        this.GameObjectId = gameObjectId;
        this.DataId = dataId;
    }

    public static bool operator ==(BaseCombatant<T>? left, BaseCombatant<T>? right) => Equals(left, right);

    public static bool operator !=(BaseCombatant<T>? left, BaseCombatant<T>? right) => !Equals(left, right);

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return this.Equals((BaseCombatant<T>)obj);
    }

    public override void CalcStats()
    {
        var duration = this.Encounter.Duration.TotalSeconds >= 1 ? this.Encounter.Duration.TotalSeconds : 1;
        this.Dps = Math.Round(this.DamageTotal / duration, 2);
        this.CritPct = Math.Round(((double)this.CritHits / this.Casts) * 100, 2);
        this.DHitPct = Math.Round(((double)this.DirectHits / this.Casts) * 100, 2);
        this.CritDHitPct = Math.Round(((double)this.CritDirectHits / this.Casts) * 100, 2);

        this.Hps = Math.Round(this.HealingTotal / duration, 2);
        this.EffectiveHealing = this.HealingTotal - this.OverHealTotal;
        this.OverHealPct = this.HealingTotal == 0 ? 0 : Math.Round(((double)this.OverHealTotal / this.HealingTotal) * 100, 2);
    }

    public override void PostCalcStats() => this.DamagePct = this.DamageTotal == 0 ? 0 : Math.Round(((double)this.DamageTotal / this.Encounter.DamageTotal) * 100, 2);

    public bool Equals(Combatant other)
    {
        if (this.DataId != 0)
        {
            return this.DataId == other.DataId;
        }

        if (this.GameObjectId != 0)
        {
            return this.GameObjectId == other.GameObjectId;
        }

        return false;
    }

    public override int GetHashCode() => HashCode.Combine(this.GameObjectId, this.DataId);

    protected bool Equals(BaseCombatant<T> other)
    {
        if (this.DataId != other.DataId)
        {
            return false;
        }

        if (this.DataId == 0)
        {
            return this.GameObjectId.Equals(other.GameObjectId);
        }

        return this.DataId.Equals(other.DataId);
    }
}