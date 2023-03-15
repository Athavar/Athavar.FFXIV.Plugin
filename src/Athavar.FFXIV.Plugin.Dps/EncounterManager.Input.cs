// <copyright file="Input.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Dalamud.Game.ClientState.Objects.Types;

internal sealed partial class EncounterManager
{
    private void OnCombatEvent(object? sender, CombatEvent @event)
    {
        if (this.CurrentEncounter is null || this.CurrentEncounter.Start == DateTime.MinValue)
        {
            this.StartEncounter(@event.Timestamp);
        }

        var encounter = this.CurrentEncounter;

        var actor = encounter.GetCombatant(@event.ActorId);
        if (actor is null)
        {
            return;
        }

        switch (@event)
        {
            case CombatEvent.Action action:
            {
                var overWriteSource = action.ActorId;
                if (!this.ci.IsPvP() && this.limitBreaks.Contains(action.ActionId.Id))
                {
                    actor = encounter.GetCombatant(uint.MaxValue);
                    overWriteSource = uint.MaxValue;
                }

                actor?.CastAction(action.ActionId.Id);

                foreach (var effectEvent in action.Effects)
                {
                    effectEvent.SourceId = overWriteSource;
                    this.HandleActionEffect(encounter, actor, action, effectEvent);
                }

                break;
            }
            case CombatEvent.DeferredEvent deferredEvent:
            {
                var effect = deferredEvent.EffectEvent;
                if (effect is null)
                {
                    return;
                }

                actor = encounter.GetCombatant(effect.SourceId);
                if (actor is null)
                {
                    return;
                }

                this.HandleActionEffect(encounter, actor, deferredEvent, effect);
                break;
            }
            case CombatEvent.StatusEffect statusEffect:
            {
                var source = encounter.GetCombatant(statusEffect.SourceId);
                if (statusEffect.Grain)
                {
                    this.Log.Add($"{@event.Timestamp:O}|EffectGrain|{source?.Name}|{actor.Name}|{this.utils.StatusString(statusEffect.StatusId)}|");
                    actor.StatusList.Add(statusEffect);
                }
                else
                {
                    this.Log.Add($"{@event.Timestamp:O}|EffectRemove|{source?.Name}|{actor.Name}|{this.utils.StatusString(statusEffect.StatusId)}|");
                    actor.StatusList.RemoveAll(e => e.StatusId == statusEffect.StatusId && e.SourceId == statusEffect.SourceId);
                }

                break;
            }
            case CombatEvent.Death deathEvent:
            {
                var source = encounter.GetCombatant(deathEvent.SourceId);
                actor.Deaths++;
                if (source != null)
                {
                    source.Kills++;
                }

                this.Log.Add($"{@event.Timestamp:O}|Kill|{source?.Name}|{actor.Name}||");
                break;
            }
        }
    }

    private void HandleActionEffect(Encounter encounter, Combatant? source, CombatEvent @event, CombatEvent.ActionEffectEvent effectEvent)
    {
        var target = encounter.GetCombatant(effectEvent.TargetId);
        if (source is null)
        {
            return;
        }

        if (effectEvent is CombatEvent.Heal heal)
        {
            var gameObject = this.objectTable?.SearchById(heal.TargetId);
            if (gameObject is BattleChara battleChara)
            {
                var missingHp = battleChara.CurrentHp > battleChara.MaxHp ? 0U : battleChara.MaxHp - battleChara.CurrentHp;
                heal.Overheal += heal.Amount < missingHp ? 0U : heal.Amount - missingHp;
            }
        }

        var action = @event as CombatEvent.Action;

        switch (effectEvent)
        {
            case CombatEvent.DamageTaken damageTakenEvent:
            {
                var isStatus = false;
                if (action is not null)
                {
                    if (damageTakenEvent.IsSourceEntry)
                    {
                        var effect = target?.StatusList.LastOrDefault(x => this.damageReceivedProcs.Contains(x.StatusId) && x.Timestamp.AddSeconds(x.Duration + 1) > action.Timestamp);
                        if (effect is not null)
                        {
                            damageTakenEvent.ActionId = effect.StatusId;
                            isStatus = true;
                        }
                    }
                }

                source.AddActionDone(@event.Timestamp, damageTakenEvent, isStatus);
                target?.AddActionTaken(@event.Timestamp, damageTakenEvent, isStatus);
                this.UpdateLastEvent(encounter, @event.Timestamp, true);

                this.Log.Add($"{@event.Timestamp:O}|Damage|{source.Name}|{target?.Name}|{this.utils.ActionString(action.ActionId)}|{damageTakenEvent.Amount}");

                break;
            }
            case CombatEvent.DoT dotEvent:
            {
                source.AddActionDone(@event.Timestamp, dotEvent);
                target?.AddActionTaken(@event.Timestamp, dotEvent);
                this.UpdateLastEvent(encounter, @event.Timestamp);

                this.Log.Add($"{@event.Timestamp:O}|DoT|{source.Name}|{target?.Name}|{this.utils.StatusString(dotEvent.StatusId)}|{dotEvent.Amount}");
                break;
            }
            case CombatEvent.HoT hotEvent:
            {
                source.AddActionDone(@event.Timestamp, hotEvent);
                target?.AddActionTaken(@event.Timestamp, hotEvent);
                this.UpdateLastEvent(encounter, @event.Timestamp);

                this.Log.Add($"{@event.Timestamp:O}|HoT|{source.Name}|{target?.Name}|{this.utils.StatusString(hotEvent.StatusId)}|{hotEvent.Amount}");
                break;
            }
            case CombatEvent.Healed healEvent:
            {
                var isStatus = false;
                if (action is not null)
                {
                    var indexOfHealEffects = Array.IndexOf(action.Effects.OfType<CombatEvent.Healed>().ToArray(), healEvent);

                    // check heal proc
                    if (healEvent.IsSourceEntry)
                    {
                        var effect = source.StatusList.LastOrDefault(x => this.damageDealtHealProcs.Contains(x.StatusId) && x.Timestamp.AddSeconds(x.Duration + 1) > action.Timestamp);
                        if (effect is not null)
                        {
                            healEvent.ActionId = effect.StatusId;
                            isStatus = true;
                        }
                    }
                    else
                    {
                        if (indexOfHealEffects >= (action.Definition?.IsHeal == true ? 1 : 0))
                        {
                            var effect = source.StatusList.LastOrDefault(x => (this.damageReceivedHealProcs.Contains(x.StatusId) || this.healCastHealProcs.Contains(x.StatusId)) && x.Timestamp.AddSeconds(x.Duration + 1) > action.Timestamp);
                            if (effect is not null)
                            {
                                healEvent.ActionId = effect.StatusId;
                                isStatus = true;
                            }
                        }
                    }
                }

                source.AddActionDone(@event.Timestamp, healEvent, isStatus);
                target?.AddActionTaken(@event.Timestamp, healEvent, isStatus);
                this.UpdateLastEvent(encounter, @event.Timestamp);

                this.Log.Add($"{@event.Timestamp:O}|Heal|{source.Name}|{target?.Name}|{this.utils.ActionString(action.ActionId)}|{healEvent.Amount}");
                break;
            }
        }
    }

    private void UpdateLastEvent(Encounter encounter, DateTime time, bool isDamage = false)
    {
        encounter.LastEvent = time;
        if (isDamage)
        {
            encounter.LastDamageEvent = time;
        }
    }
}