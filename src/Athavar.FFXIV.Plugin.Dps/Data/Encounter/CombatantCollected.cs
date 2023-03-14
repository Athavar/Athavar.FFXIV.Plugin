// <copyright file="CombatantCollected.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

using System.Runtime.InteropServices;

internal sealed class CombatantCollected : BaseCombatant<CombatantCollected>
{
    private readonly List<Combatant> combatants = new();

    public CombatantCollected(TerritoryEncounter encounter, Combatant combatant)
        : base(encounter, combatant.ObjectId, combatant.DataId)
    {
        this.combatants.Add(combatant);

        this.Name = combatant.Name;
        this.OwnerId = combatant.OwnerId;
        this.Name_First = combatant.Name_First;
        this.Name_Last = combatant.Name_First;
        this.Name_First = combatant.Name_First;
        this.Name_First = combatant.Name_First;
        this.Job = combatant.Job;
        this.Level = combatant.Level;
        this.CurrentWorldId = combatant.CurrentWorldId;
        this.WorldId = combatant.WorldId;
        this.WorldName = combatant.WorldName;
        this.PartyType = combatant.PartyType;
        this.Kind = combatant.Kind;
    }

    public override void CalcStats()
    {
        ulong damageTotal = 0,
              damageTaken = 0,
              healingTotal = 0,
              healingTaken = 0,
              hits = 0,
              crit = 0,
              direct = 0,
              critDirect = 0;
        ushort deaths = 0,
               kills = 0;
        foreach (var combatant in CollectionsMarshal.AsSpan(this.combatants))
        {
            damageTotal += combatant.DamageTotal;
            damageTaken += combatant.DamageTaken;
            healingTotal += combatant.HealingTotal;
            healingTaken += combatant.HealingTaken;

            hits += combatant.Casts;
            crit += combatant.CritHits;
            direct += combatant.DirectHits;
            critDirect += combatant.CritDirectHits;

            deaths += combatant.Deaths;
            kills += combatant.Kills;
        }

        this.DamageTotal = damageTotal;
        this.DamageTaken = damageTaken;
        this.HealingTotal = healingTotal;
        this.HealingTaken = healingTaken;

        this.Casts = hits;
        this.CritHits = crit;
        this.DirectHits = direct;
        this.CritDirectHits = critDirect;

        this.Deaths = deaths;
        this.Kills = kills;

        base.CalcStats();
    }

    public void AddCombatant(Combatant baseCombatant) => this.combatants.Add(baseCombatant);
}