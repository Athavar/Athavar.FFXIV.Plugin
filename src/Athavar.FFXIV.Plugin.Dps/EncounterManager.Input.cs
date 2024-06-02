// <copyright file="EncounterManager.Input.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Common.Definitions;
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
                    effectEvent.EffectSourceId = overWriteSource;
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
                    var definition = this.definitions.GetStatusEffectById(statusEffect.StatusId);
                    actor.StatusList.Add(new StatusEntry(statusEffect, definition));
                }
                else
                {
                    this.Log.Add($"{@event.Timestamp:O}|EffectRemove|{source?.Name}|{actor.Name}|{this.utils.StatusString(statusEffect.StatusId)}|");
                    actor.StatusList.RemoveAll(e => e.Effect.StatusId == statusEffect.StatusId && e.Effect.SourceId == statusEffect.SourceId);
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

                this.UpdateLastEvent(encounter, @event.Timestamp, true);

                this.Log.Add($"{@event.Timestamp:O}|Kill|{source?.Name}|{actor.Name}||");
                break;
            }
        }
    }

    private void HandleActionEffect(Encounter encounter, Combatant? source, CombatEvent @event, CombatEvent.ActionEffectEvent effectEvent)
    {
        var target = encounter.GetCombatant(effectEvent.TargetId);

        if (effectEvent.IsSourceTarget && effectEvent.SourceId != source?.ObjectId)
        {
            // reflect effect
            source = encounter.GetCombatant(effectEvent.SourceId);
        }

        if (source is null)
        {
            return;
        }

        if (effectEvent is CombatEvent.Heal heal)
        {
            var gameObject = this.objectTable.SearchById(heal.TargetId);
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
                        var effect = source.StatusList.LastOrDefault(
                            x => x.Definition?.ReactiveProc?.Type == ReactiveProc.ReactiveProcType.DamageOnDamageReceived
                              && x.Effect.Timestamp.AddSeconds(x.Effect.Duration + 1) > action.Timestamp);
                        if (effect is not null)
                        {
                            damageTakenEvent.ActionId = effect.Effect.StatusId;
                            isStatus = true;
                        }
                    }
                }

                // filter out cases without direct "killer"
                if (effectEvent.IsSourceTarget || source != target)
                {
                    source.AddActionDone(@event.Timestamp, damageTakenEvent, isStatus);
                }

                target?.AddActionTaken(@event.Timestamp, damageTakenEvent, isStatus);
                this.UpdateLastEvent(encounter, @event.Timestamp, true);

                this.Log.Add($"{@event.Timestamp:O}|Damage{(damageTakenEvent.IsSourceTarget ? "(reflect/self)" : string.Empty)}|{source.Name}|{target?.Name}|{(action is not null ? this.utils.ActionString(action.ActionId) : string.Empty)}|{damageTakenEvent.Amount}");
                if (effectEvent.IsSourceTarget)
                {
                    this.Log.Add($"{damageTakenEvent.EffectSourceId}!={effectEvent.SourceId} && {damageTakenEvent.EffectTargetId}!={effectEvent.TargetId}");
                }

                break;
            }

            case CombatEvent.DoT dotEvent:
            {
                var handled = false;
                if (dotEvent.StatusId == 0)
                {
                    // event is not assigned to a specific source
                    var targetObject = this.objectTable.SearchById(dotEvent.TargetId);
                    if (targetObject is BattleChara battleChara)
                    {
                        var affectedStatusList = battleChara.StatusList.Select(s => (Status: this.definitions.GetStatusEffectById(s.StatusId), Source: encounter.GetCombatant(s.SourceId))).Where(s => s.Status?.TimeProc?.Type is TimeProc.TickType.DoT).ToList();
                        if (affectedStatusList.Any())
                        {
                            // only calculate amount based on potency. Buffs are not calculated in.
                            handled = true;
                            var combinedPotency = affectedStatusList.Sum(s => s.Status!.TimeProc!.Potency);
                            var totalAmount = dotEvent.Amount;
                            foreach (var (status, combatant) in affectedStatusList)
                            {
                                var multi = (double)status!.TimeProc!.Potency / combinedPotency;
                                var calc = (uint)(totalAmount * multi);
                                if (calc == 0)
                                {
                                    continue;
                                }

                                dotEvent.Amount = calc;
                                dotEvent.StatusId = status.Id;
                                combatant?.AddActionDone(@event.Timestamp, dotEvent);
                                target?.AddActionTaken(@event.Timestamp, dotEvent);
                                this.Log.Add($"{@event.Timestamp:O}|DoT|{combatant?.Name}|{target?.Name}|{this.utils.StatusString(dotEvent.StatusId)}|{dotEvent.Amount} of {totalAmount}");
                            }
                        }

                        // TODO: Fix dot assignment on not enemy players
                    }
                }

                if (!handled)
                {
                    // fallback, that data is not dropped.
                    if (target?.IsEnemy() != source.IsEnemy())
                    {
                        // no friendly fire
                        source.AddActionDone(@event.Timestamp, dotEvent);
                    }

                    target?.AddActionTaken(@event.Timestamp, dotEvent);
                    this.Log.Add($"{@event.Timestamp:O}|DoT|{source.Name}|{target?.Name}|{this.utils.StatusString(dotEvent.StatusId)}|{dotEvent.Amount}");
                }

                this.UpdateLastEvent(encounter, @event.Timestamp);
                break;
            }

            case CombatEvent.HoT hotEvent:
            {
                var handled = false;
                if (hotEvent.StatusId == 0)
                {
                    // event is not assigned to a specific source
                    var targetObject = this.objectTable.SearchById(hotEvent.TargetId);
                    if (targetObject is BattleChara battleChara)
                    {
                        var affectedStatusList = battleChara.StatusList.Select(s => (Status: this.definitions.GetStatusEffectById(s.StatusId), Source: encounter.GetCombatant(s.SourceId))).Where(s => s.Status?.TimeProc?.Type is TimeProc.TickType.HoT).ToList();
                        if (affectedStatusList.Any())
                        {
                            // only calculate amount based on potency. Buffs are not calculated in.
                            handled = true;
                            var combinedPotency = affectedStatusList.Sum(s => s.Status!.TimeProc!.Potency);
                            var totalHeal = hotEvent.Amount;
                            var totalOverHeal = hotEvent.Overheal;
                            foreach (var (status, combatant) in affectedStatusList)
                            {
                                var multi = (double)status!.TimeProc!.Potency / combinedPotency;
                                var calcHeal = (uint)(totalHeal * multi);
                                var calcOverHeal = (uint)(totalOverHeal * multi);
                                if (calcHeal == 0)
                                {
                                    continue;
                                }

                                hotEvent.Amount = calcHeal;
                                hotEvent.Overheal = calcOverHeal;
                                hotEvent.StatusId = status.Id;
                                combatant?.AddActionDone(@event.Timestamp, hotEvent);
                                target?.AddActionTaken(@event.Timestamp, hotEvent);
                                this.Log.Add($"{@event.Timestamp:O}|HoT|{combatant?.Name}|{target?.Name}|{this.utils.StatusString(hotEvent.StatusId)}|{hotEvent.Amount} of {totalHeal}");
                            }
                        }
                    }
                }

                if (!handled)
                {
                    // fallback, that data is not dropped.
                    source.AddActionDone(@event.Timestamp, hotEvent);
                    target?.AddActionTaken(@event.Timestamp, hotEvent);
                    this.Log.Add($"{@event.Timestamp:O}|HoT|{source.Name}|{target?.Name}|{this.utils.StatusString(hotEvent.StatusId)}|{hotEvent.Amount}");
                }

                this.UpdateLastEvent(encounter, @event.Timestamp);
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
                        var effect = source.StatusList.LastOrDefault(
                            x => x.Definition?.ReactiveProc?.Type == ReactiveProc.ReactiveProcType.HealOnDamageDealt
                              && x.Effect.Timestamp.AddSeconds(x.Effect.Duration + 1) > action.Timestamp);
                        if (effect is not null)
                        {
                            healEvent.ActionId = effect.Effect.StatusId;
                            isStatus = true;
                        }
                    }
                    else
                    {
                        if (indexOfHealEffects >= (action.Definition?.IsHeal == true ? 1 : 0))
                        {
                            var effect = source.StatusList.LastOrDefault(
                                x => x.Definition?.ReactiveProc?.Type is ReactiveProc.ReactiveProcType.HealOnDamageReceived or ReactiveProc.ReactiveProcType.HealOnHealCast
                                  && x.Effect.Timestamp.AddSeconds(x.Effect.Duration + 1) > action.Timestamp);
                            if (effect is not null)
                            {
                                healEvent.ActionId = effect.Effect.StatusId;
                                isStatus = true;
                            }
                        }
                    }
                }

                source.AddActionDone(@event.Timestamp, healEvent, isStatus);
                target?.AddActionTaken(@event.Timestamp, healEvent, isStatus);
                this.UpdateLastEvent(encounter, @event.Timestamp);

                this.Log.Add($"{@event.Timestamp:O}|Heal|{source.Name}|{target?.Name}|{(action is not null ? this.utils.ActionString(action.ActionId) : string.Empty)}|{healEvent.Amount}");
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