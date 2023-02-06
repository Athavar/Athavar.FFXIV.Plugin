// <copyright file="CombatantCollected.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using System.Runtime.InteropServices;

internal class CombatantCollected : BaseCombatant<CombatantCollected>
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
        ulong damageTotal = 0;
        ulong damageTaken = 0;
        ulong healingTotal = 0;
        ulong healingTaken = 0;
        ushort deaths = 0;
        ushort kills = 0;
        foreach (var combatant in CollectionsMarshal.AsSpan(this.combatants))
        {
            damageTotal += combatant.DamageTotal;
            damageTaken += combatant.DamageTaken;
            healingTotal += combatant.HealingTotal;
            healingTaken += combatant.HealingTaken;

            this.Deaths += combatant.Deaths;
            this.Kills += combatant.Kills;
        }

        this.DamageTotal = damageTotal;
        this.DamageTaken = damageTaken;
        this.HealingTotal = healingTotal;
        this.HealingTaken = healingTaken;
        this.Deaths = deaths;
        this.Kills = kills;

        base.CalcStats();
    }

    public void AddCombatant(Combatant baseCombatant) => this.combatants.Add(baseCombatant);
}