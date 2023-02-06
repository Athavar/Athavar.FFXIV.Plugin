// <copyright file="BaseEncounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using Athavar.FFXIV.Plugin.Config;
using Dalamud.Game.ClientState.Objects.Enums;

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

    public abstract void CalcStats(PartyType filter);

    protected List<T> GetFilteredCombatants(PartyType filter)
        => this.Combatants
           .Where(c => c is not { DamageTaken: 0, HealingTotal: 0, DamageTotal: 0 } && c.Kind != BattleNpcSubKind.Enemy)
           .Where(c => c.PartyType <= filter)
           .ToList();
}