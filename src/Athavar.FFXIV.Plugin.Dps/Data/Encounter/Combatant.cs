// <copyright file="Combatant.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Dps.Data.ActionSummary;

internal class Combatant : BaseCombatant<Combatant>
{
    public readonly List<CombatEvent.StatusEffect> StatusList = new();

    private readonly List<ActionSummary> actions = new();

    private bool change;

    internal Combatant(Encounter encounter, uint objectId, uint dataId)
        : base(encounter, objectId, dataId)
    {
        this.Name = string.Empty;
        this.Name_First = string.Empty;
        this.Name_Last = string.Empty;
    }

    public int ActionCount => this.actions.Count;

    public IReadOnlyList<ActionSummary> Actions => this.actions;

    public override void CalcStats()
    {
        if (this.change)
        {
            this.change = false;
            ulong healingTotal = 0;
            ulong overHealing = 0;
            ulong healingTaken = 0;
            ulong damageTotal = 0;
            ulong damageTaken = 0;
            foreach (var actionSummary in CollectionsMarshal.AsSpan(this.actions))
            {
                actionSummary.Calc();
                var value = actionSummary.HealingDone;
                if (value is not null)
                {
                    healingTotal += value.TotalAmount;
                    overHealing += value.OverAmount;
                }

                value = actionSummary.HealingTaken;
                if (value is not null)
                {
                    healingTaken += value.TotalAmount;
                }

                value = actionSummary.DamageDone;
                if (value is not null)
                {
                    damageTotal += value.TotalAmount;
                }

                value = actionSummary.DamageTaken;
                if (value is not null)
                {
                    damageTaken += value.TotalAmount;
                }
            }

            this.HealingTotal = healingTotal;
            this.HealingTaken = healingTaken;
            this.OverHealTotal = overHealing;
            this.DamageTotal = damageTotal;
            this.DamageTaken = damageTaken;
        }

        base.CalcStats();
    }

    public void CastAction(uint id) => this.GetActionSummary(id).OnCast();

    public void AddActionTaken(DateTime timestamp, CombatEvent.ActionEffectEvent effectEvent, bool isStatusOverwrite = false)
    {
        switch (effectEvent)
        {
            case CombatEvent.HoT hoT:
            {
                var summary = this.GetActionSummary(hoT.StatusId, true);
                summary.OnHealingTake(new ActionEvent(timestamp, hoT.SourceId, effectEvent.GetModifier(), hoT.Amount, hoT.Overheal));
                break;
            }
            case CombatEvent.Healed healed:
            {
                var summary = this.GetActionSummary(healed.ActionId, isStatusOverwrite);
                summary.OnHealingTake(new ActionEvent(timestamp, healed.SourceId, effectEvent.GetModifier(), healed.Amount, healed.Overheal));
                break;
            }
            case CombatEvent.DoT doT:
            {
                var summary = this.GetActionSummary(doT.StatusId, true);
                summary.OnDamageTake(new ActionEvent(timestamp, doT.SourceId, effectEvent.GetModifier(), doT.Amount));
                break;
            }
            case CombatEvent.DamageTaken damage:
            {
                var summary = this.GetActionSummary(damage.ActionId, isStatusOverwrite);
                summary.OnDamageTake(new ActionEvent(timestamp, damage.SourceId, effectEvent.GetModifier(), damage.Amount));
                break;
            }
        }

        this.change = true;
    }

    public void AddActionDone(DateTime timestamp, CombatEvent.ActionEffectEvent effectEvent, bool isStatusOverwrite = false)
    {
        switch (effectEvent)
        {
            case CombatEvent.HoT hoT:
            {
                var summary = this.GetActionSummary(hoT.StatusId, true);
                summary.OnHealingDone(new ActionEvent(timestamp, hoT.TargetId, effectEvent.GetModifier(), hoT.Amount, hoT.Overheal));
                break;
            }
            case CombatEvent.Healed healed:
            {
                var summary = this.GetActionSummary(healed.ActionId, isStatusOverwrite);
                summary.OnHealingDone(new ActionEvent(timestamp, healed.TargetId, effectEvent.GetModifier(), healed.Amount, healed.Overheal));
                break;
            }
            case CombatEvent.DoT doT:
            {
                var summary = this.GetActionSummary(doT.StatusId, true);
                summary.OnDamageDone(new ActionEvent(timestamp, doT.TargetId, effectEvent.GetModifier(), doT.Amount));
                break;
            }
            case CombatEvent.DamageTaken damage:
            {
                var summary = this.GetActionSummary(damage.ActionId, isStatusOverwrite);
                summary.OnDamageDone(new ActionEvent(timestamp, damage.TargetId, effectEvent.GetModifier(), damage.Amount));
                break;
            }
        }

        this.change = true;
    }

    private ActionSummary GetActionSummary(uint id, bool isStatus = false)
    {
        var action = this.actions.Find(a => a.Id == id && a.IsStatus == isStatus);
        if (action is null)
        {
            action = new ActionSummary(id, isStatus);
            this.actions.Add(action);
        }

        return action;
    }
}