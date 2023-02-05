// <copyright file="Input.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Dps.Data;
using Dalamud.Game.ClientState.Objects.Types;

internal partial class EncounterManager
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
                if (statusEffect.Grain)
                {
                    this.Log.Add($"[{@event.Timestamp:O}] EffectGrain: {actor} -> {this.utils.StatusString(statusEffect.StatusId)}");
                    actor.StatusList.Add(statusEffect);
                }
                else
                {
                    this.Log.Add($"[{@event.Timestamp:O}] EffectRemove: {actor} -> {this.utils.StatusString(statusEffect.StatusId)}");
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

                this.Log.Add($"[{@event.Timestamp:O}] Kill: {source} -> {actor}");

                break;
            }
        }
    }

    private void HandleActionEffect(Encounter encounter, Combatant? source, CombatEvent @event, CombatEvent.ActionEffectEvent effectEvent)
    {
        Combatant? target;

        if (source is null)
        {
            return;
        }

        var action = @event as CombatEvent.Action;

        switch (effectEvent)
        {
            case CombatEvent.DamageTaken damageTakenEvent:
            {
                if (action is not null)
                {
                    if (damageTakenEvent.IsSourceEntry)
                    {
                        target = encounter.GetCombatant(damageTakenEvent.TargetId);
                        var effect = target?.StatusList.LastOrDefault(x => this.damageReceivedProcs.Contains(x.StatusId) && x.Timestamp.AddSeconds(x.Duration + 1) > action.Timestamp);
                        if (effect is not null)
                        {
                            damageTakenEvent.ActionName = this.utils.StatusString(effect.StatusId);
                            damageTakenEvent.SourceId = effect.SourceId;
                            damageTakenEvent.TargetId = action.ActorId;
                        }
                    }
                }

                target = encounter.GetCombatant(effectEvent.TargetId);
                source = encounter.GetCombatant(effectEvent.SourceId);

                if (target != null)
                {
                    target.DamageTaken += damageTakenEvent.Amount;
                }

                if (source != null)
                {
                    source.DamageTotal += damageTakenEvent.Amount;
                }

                encounter.LastEvent = encounter.LastDamageEvent = @event.Timestamp;

                this.Log.Add($"[{@event.Timestamp:O}] Damage: {source} -> {target} => {damageTakenEvent.Amount}");

                break;
            }
            case CombatEvent.DoT dotEvent:
            {
                target = encounter.GetCombatant(dotEvent.TargetId);
                if (target != null)
                {
                    target.DamageTaken += dotEvent.Amount;
                }

                source.DamageTotal += dotEvent.Amount;
                encounter.LastEvent = @event.Timestamp;

                this.Log.Add($"[{@event.Timestamp:O}] Dot: {source} -> {target} => {dotEvent.Amount}");
                break;
            }
            case CombatEvent.HoT hotEvent:
            {
                target = encounter.GetCombatant(hotEvent.TargetId);
                this.ApplyHeal(source, target, hotEvent.TargetId, hotEvent.Amount);
                encounter.LastEvent = @event.Timestamp;

                this.Log.Add($"[{@event.Timestamp:O}] Hot: {source} -> {target} => {hotEvent.Amount}");
                break;
            }
            case CombatEvent.Healed healEvent:
            {
                var targetId = healEvent.TargetId;
                if (action is not null)
                {
                    var indexOfHealEffects = Array.IndexOf(action.Effects.OfType<CombatEvent.Healed>().ToArray(), healEvent);

                    // check heal proc
                    if (healEvent.IsSourceEntry)
                    {
                        var status = source.StatusList.LastOrDefault(x => this.damageDealtHealProcs.Contains(x.StatusId));
                        this.Log.Add($"Prop Prog? {status} {status?.Timestamp.AddSeconds(status.Duration + 1):T} {action.Timestamp:T}");
                        var effect = source.StatusList.LastOrDefault(x => this.damageDealtHealProcs.Contains(x.StatusId) && x.Timestamp.AddSeconds(x.Duration + 1) > action.Timestamp);
                        if (effect is not null)
                        {
                            healEvent.ActionName = this.utils.StatusString(effect.StatusId);
                            healEvent.SourceId = effect.SourceId;
                            healEvent.TargetId = action.ActorId;
                            targetId = action.ActorId;
                        }
                    }
                    else
                    {
                        if (indexOfHealEffects >= (action.Definition?.IsHeal == true ? 1 : 0))
                        {
                            var effect = source.StatusList.LastOrDefault(x => (this.damageReceivedHealProcs.Contains(x.StatusId) || this.healCastHealProcs.Contains(x.StatusId)) && x.Timestamp.AddSeconds(x.Duration + 1) > action.Timestamp);
                            if (effect is not null)
                            {
                                healEvent.ActionName = this.utils.StatusString(effect.StatusId);
                                healEvent.SourceId = effect.SourceId;
                                healEvent.TargetId = action.TargetId;
                                targetId = action.TargetId;
                            }
                        }
                    }

                    // check Clemency
                    if (action.ActionId == ActionId.Clemency && indexOfHealEffects == 1)
                    {
                        healEvent.TargetId = action.ActorId;
                        targetId = action.ActorId;
                    }
                }

                source = encounter.GetCombatant(healEvent.SourceId);
                target = encounter.GetCombatant(healEvent.TargetId);
                this.ApplyHeal(source, target, targetId, healEvent.Amount);
                encounter.LastEvent = @event.Timestamp;

                this.Log.Add($"[{@event.Timestamp:O}] Heal: {source} -> {target} => {healEvent.Amount}");

                break;
            }
        }
    }

    private void ApplyHeal(Combatant? source, Combatant? target, uint targetId, uint healAmount)
    {
        if (source == null)
        {
            return;
        }

        source.HealingTotal += healAmount;
        if (target != null)
        {
            target.HealingTaken += healAmount;
        }

        var gameObject = this.objectTable.SearchById(targetId);
        if (gameObject is BattleChara battleChara)
        {
            var missingHp = battleChara.CurrentHp > battleChara.MaxHp ? 0U : battleChara.MaxHp - battleChara.CurrentHp;
            source.OverHealTotal += healAmount < missingHp ? 0U : healAmount - missingHp;
        }
    }
}