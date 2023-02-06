// <copyright file="Combatant.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

internal class Combatant : BaseCombatant<Combatant>
{
    public readonly List<CombatEvent.StatusEffect> StatusList = new();

    internal Combatant(Encounter encounter, uint objectId, uint dataId)
        : base(encounter, objectId, dataId)
    {
        this.Name = string.Empty;
        this.Name_First = string.Empty;
        this.Name_Last = string.Empty;
    }
}