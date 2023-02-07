// <copyright file="TerritoryEncounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

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
        encounter.SetTerritoryEncounter(this);
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

    public override void CalcStats()
    {
        base.CalcStats();
        
        var encounters = this.Encounters;

        var duration = TimeSpan.Zero;
        foreach (var encounter in encounters)
        {
            encounter.CalcStats();
            duration += encounter.Duration;
            this.LastEvent = encounter.LastEvent;
            encounter.CalcPostStats();
        }

        this.duration = duration;

        //  PluginLog.LogInformation($"UpdateAllies: {string.Join(',', this.Combatants.Select(c => $"{c.ObjectId}:{c.Name} -> {c.Kind.AsText()}"))}");
    }

    public override bool IsValid() => true;
}