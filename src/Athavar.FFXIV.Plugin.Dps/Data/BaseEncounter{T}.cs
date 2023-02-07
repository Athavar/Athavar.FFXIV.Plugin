// <copyright file="BaseEncounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using System.Runtime.InteropServices;

internal abstract class BaseEncounter<T> : BaseEncounter
    where T : BaseCombatant
{
    public BaseEncounter()
    {
    }

    public BaseEncounter(string territoryName, ushort territory, DateTime start)
        : base(territoryName, territory, start)
    {
    }

    public List<T> Combatants { get; protected set; } = new();

    public List<T> AllyCombatants { get; protected set; } = new();

    public override IEnumerable<BaseCombatant> GetCombatants() => this.Combatants;

    public override IEnumerable<BaseCombatant> GetAllyCombatants() => this.AllyCombatants;

    public virtual void CalcStats()
    {
        var combatants = this.Combatants;
        double dps = 0;
        double hps = 0;
        ulong damageTotal = 0;
        ulong damageTaken = 0;
        ulong damageHeal = 0;
        var deaths = 0;
        var kills = 0;
        List<T> allies = new();
        foreach (var combatant in CollectionsMarshal.AsSpan(combatants))
        {
            combatant.CalcStats();
            if (!combatant.IsActive() || combatant.IsEnemy() || !combatant.IsAlly(this.Filter))
            {
                continue;
            }

            allies.Add(combatant);

            dps += combatant.Dps;
            hps += combatant.Hps;
            damageTotal += combatant.DamageTotal;
            damageTaken += combatant.DamageTaken;
            damageHeal += combatant.HealingTotal;
            deaths += combatant.Deaths;
            kills += combatant.Kills;
        }

        this.Dps = Math.Round(dps, 2);
        this.Hps = Math.Round(hps, 2);
        this.DamageTotal = damageTotal;
        this.DamageTaken = damageTaken;
        this.HealingTotal = damageHeal;
        this.Deaths = deaths;
        this.Kills = kills;

        this.AllyCombatants = allies;
    }

    public void CalcPostStats()
    {
        var combatants = this.AllyCombatants;
        foreach (var combatant in CollectionsMarshal.AsSpan(combatants))
        {
            combatant.PostCalcStats();
        }
    }
}