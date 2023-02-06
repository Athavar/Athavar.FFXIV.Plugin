// <copyright file="TerritoryEncounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using Athavar.FFXIV.Plugin.Config;

internal class TerritoryEncounter : BaseEncounter<CombatantCollected>
{
    public List<Encounter> Encounters = new();

    public bool closed;

    private TimeSpan duration;

    public TerritoryEncounter()
    {
    }

    public TerritoryEncounter(string territoryName, ushort territory, DateTime start)
        : base(territoryName, territory, start)
    {
    }

    public override string Name => this.TerritoryName;

    public override TimeSpan Duration => this.duration;
    
    public TimeSpan DurationTotal => this.End is null ? this.LastEvent - this.Start : this.End.Value - this.Start;

    public override string? Title => this.TerritoryName;

    public void AddEncounter(Encounter encounter)
    {
        encounter.SetTerritoryEncounter();
        this.Encounters.Add(encounter);
        foreach (var combatant in encounter.Combatants)
        {
            this.AddCombatant(combatant);
        }
    }

    public void AddCombatant(Combatant baseCombatant)
    {
        var find = this.Combatants.Find(c => c.Equals(baseCombatant));
        if (find is null)
        {
            this.Combatants.Add(new CombatantCollected(this, baseCombatant));
        }
        else
        {
            find.AddCombatant(baseCombatant);
        }
    }

    public void EndEncounter()
    {
        this.closed = true;
        this.End = this.Encounters.Last().End;
    }
    

    public override void CalcStats(PartyType filter)
    {
        var encounters = this.Encounters;

        double dps = 0;
        double hps = 0;
        ulong damageTotal = 0;
        var deaths = 0;
        var kills = 0;
        var duration = TimeSpan.Zero;
        foreach (var encounter in encounters)
        {
            encounter.CalcStats(filter);
            dps += encounter.Dps;
            hps += encounter.Hps;
            damageTotal += encounter.DamageTotal;
            deaths += encounter.Deaths;
            kills += encounter.Kills;

            duration = duration.Add(this.Duration);
            this.LastEvent = encounter.LastEvent;
        }

        this.Dps = Math.Round(dps, 2);
        this.Hps = Math.Round(hps, 2);
        this.DamageTotal = damageTotal;
        this.Deaths = deaths;
        this.Kills = kills;
        this.duration = duration;

        // PluginLog.LogInformation($"UpdateAllies: {string.Join(',', this.Combatants.Select(c => $"{c.ObjectId}:{c.Name} -> {c.Kind.AsText()}"))}");
        foreach (var combatant in this.Combatants)
        {
            combatant.CalcStats();
        }

        this.AllyCombatants = this.GetFilteredCombatants(filter);
    }

    public override bool IsValid() => true;
}