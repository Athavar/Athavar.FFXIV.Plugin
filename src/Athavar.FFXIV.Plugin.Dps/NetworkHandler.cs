// <copyright file="NetworkHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game;
using Machina.FFXIV.Headers;
using Server_EffectResult = Athavar.FFXIV.Plugin.Dps.Data.Protocol.Server_EffectResult;

internal sealed partial class NetworkHandler : IDisposable
{
    private readonly IPluginLogger logger;
    private readonly IDefinitionManager definitionManager;
    private readonly EventCaptureManager eventCaptureManager;
    private readonly Utils utils;

    // this is a mega weird thing - apparently some IDs sent over network have some extra delta added to them (e.g. action ids, icon ids, etc.)
    // they change on relogs or zone changes or something...
    // we have one simple way of detecting them - by looking at casts, since they contain both offset id and real ('animation') id
    private long unkDelta;

    private Hook<ProcessPacketActionEffectDelegate>? actionEffectHook;
    private Hook<ProcessPacketEffectResultDelegate>? effectResultHook;

    public unsafe NetworkHandler(IDalamudServices dalamudServices, Utils utils, IDefinitionManager definitionManager, AddressResolver addressResolver, EventCaptureManager eventCaptureManager)
    {
        this.logger = dalamudServices.PluginLogger;
        this.utils = utils;
        this.definitionManager = definitionManager;
        this.eventCaptureManager = eventCaptureManager;

        eventCaptureManager.ActorControlEvent += this.EventCaptureManager;
        dalamudServices.SafeEnableHookFromAddress<ProcessPacketActionEffectDelegate>("NetworkHandler:actionEffectHook", addressResolver.ActionEffectHandler, this.HandleActionEffect, h => this.actionEffectHook = h);
        dalamudServices.SafeEnableHookFromAddress<ProcessPacketEffectResultDelegate>("NetworkHandler:effectResultHook", addressResolver.EffectResultHandler, this.HandleEffectResultDetour, h => this.effectResultHook = h);
    }

    private unsafe delegate void ProcessPacketActionEffectDelegate(uint sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* effectHeader, ActionEffect* effectArray, ulong* effectTrail);

    private unsafe delegate void ProcessPacketEffectResultDelegate(uint actor, Server_EffectResult* actionIntegrityData, bool isReplay);

    public event EventHandler<CombatEvent>? CombatEvent;

    public bool Debug { get; set; } = false;

    public bool Enable { get; set; } = true;

    public void Dispose()
    {
        this.actionEffectHook?.Dispose();
        this.effectResultHook?.Dispose();
        this.eventCaptureManager.ActorControlEvent -= this.EventCaptureManager;
    }

    private unsafe void HandleActionEffect(uint sourceId, IntPtr sourceCharacter, IntPtr pos, ActionEffectHeader* header, ActionEffect* effects, ulong* targetIds)
    {
        this.actionEffectHook!.OriginalDisposeSafe(sourceId, sourceCharacter, pos, header, effects, targetIds);
        var maxTargets = header->EffectCount;
        try
        {
            if (this.Debug)
            {
                this.DumpActionEffect(header, effects, targetIds, maxTargets);
            }

            if ((byte)header->EffectDisplayType == (byte)ActionType.Action)
            {
                var newDelta = (int)header->ActionId - header->ActionAnimationId;
                if (this.unkDelta != newDelta)
                {
                    this.logger.Verbose($"Updating network delta: {this.unkDelta} -> {newDelta}");
                    this.unkDelta = newDelta;
                }
            }

            var actionType = (byte)header->EffectDisplayType != (byte)ActionType.Mount
                ? (byte)header->EffectDisplayType != (byte)ActionType.Item ? ActionType.Action : ActionType.Item
                : ActionType.Mount;

            var actionId = actionType == ActionType.Action ? header->ActionAnimationId : header->ActionId;

            var actionEffects = new List<CombatEvent.ActionEffectEvent>();

            var targets = Math.Min(header->EffectCount, maxTargets);
            for (var i = 0; i < targets; ++i)
            {
                var targetId = (uint)(targetIds[i] & uint.MaxValue);
                ActionEffects actionEffects1;
                for (var j = 0; j < 8; ++j)
                {
                    actionEffects1[j] = *(ulong*)(effects + (i * 8) + j);
                }

                foreach (var actionEffect in actionEffects1)
                {
                    switch (actionEffect.Type)
                    {
                        case ActionEffectType.FullResist:
                        case ActionEffectType.Miss:
                        case ActionEffectType.Damage:
                        case ActionEffectType.BlockedDamage:
                        case ActionEffectType.ParriedDamage:
                        {
                            actionEffects.Add(
                                new CombatEvent.DamageTaken
                                {
                                    HitSeverity = actionEffect.Param0,
                                    Param1 = actionEffect.Param1,
                                    Percentage = actionEffect.Param2,
                                    Multiplier = actionEffect.Param3,
                                    Flags2 = actionEffect.Param4,
                                    Value = actionEffect.Value,

                                    EffectTargetId = targetId,
                                    EffectSourceId = sourceId,
                                    ActionId = actionId,
                                    ActionType = actionType,
                                });
                            break;
                        }

                        case ActionEffectType.Heal:
                        {
                            actionEffects.Add(
                                new CombatEvent.Healed
                                {
                                    HitSeverity = actionEffect.Param1,
                                    Param1 = actionEffect.Param0,
                                    Percentage = actionEffect.Param2,
                                    Multiplier = actionEffect.Param3,
                                    Flags2 = actionEffect.Param4,
                                    Value = actionEffect.Value,

                                    EffectTargetId = targetId,
                                    EffectSourceId = sourceId,
                                    ActionId = actionId,
                                    ActionType = actionType,
                                });
                            break;
                        }
                    }
                }
            }

            var info = new CombatEvent.Action
            {
                SequenceId = header->GlobalEffectCounter,
                ActionId = new ActionId(actionType, actionId),
                Definition = this.definitionManager.GetActionById(actionId),
                TargetId = header->AnimationTargetId,
                ActorId = sourceId,
                Effects = actionEffects.ToArray(),
            };

            this.CombatEvent?.Invoke(this, info);
        }
        catch (Exception e)
        {
            this.logger.Error(e, "Error while processing HandleActionEffect");
        }
    }

    private unsafe void HandleEffectResultDetour(uint actorId, Server_EffectResult* entries, bool isReplay)
    {
        this.effectResultHook?.OriginalDisposeSafe(actorId, entries, isReplay);
        var count = entries->EffectCount;

        try
        {
            if (this.Debug)
            {
                this.DumpEffectResult(count, entries);
            }

            var p = entries;
            for (var i = 0; i < count; ++i)
            {
                var cnt = Math.Min(4u, p->EffectCount);
                var effects = (Server_EffectResultEntry*)p->Effects;
                for (var j = 0; j < cnt; ++j)
                {
                    var eff = effects[j];
                    var effectId = eff.EffectID;
                    if (effectId <= 0)
                    {
                        continue;
                    }

                    this.CombatEvent?.Invoke(
                        this,
                        new CombatEvent.StatusEffect
                        {
                            ActorId = p->ActorID,
                            SourceId = eff.SourceActorID,
                            Grain = true,
                            StatusId = eff.EffectID,
                            Duration = eff.duration,
                        });
                }

                ++p;
            }
        }
        catch (Exception e)
        {
            this.logger.Error(e, "Error while processing HandleEffectResultDetour");
        }
    }

    private void EventCaptureManager(ActorControlCategory category, IGameObject actor, uint param1, uint param2, uint param3, uint param4)
    {
        if (this.Debug /*|| p->category is Server_ActorControlCategory.HoT or Server_ActorControlCategory.DoT*/)
        {
            this.Log($"[Network] {this.utils.ObjectString(actor)} - cat={(int)category}|{category.AsText()}, params={param1:X8} {param2:X8} {param3:X8} {param4:X8}");
        }

        var actorId = actor.EntityId;
        switch (category)
        {
            case ActorControlCategory.HoT:
                this.CombatEvent?.Invoke(
                    this,
                    new CombatEvent.DeferredEvent
                    {
                        ActorId = actorId,
                        EffectEvent = new CombatEvent.HoT
                        {
                            Value = (ushort)(param2 & ushort.MaxValue),
                            Flags2 = param2 > ushort.MaxValue ? (byte)64 : (byte)0,
                            Multiplier = (byte)(param2 / 0x10000),

                            EffectTargetId = actorId,
                            StatusId = param1,
                            EffectSourceId = param3,
                        },
                    });
                break;
            case ActorControlCategory.DoT:
                this.CombatEvent?.Invoke(
                    this,
                    new CombatEvent.DeferredEvent
                    {
                        ActorId = actorId,
                        EffectEvent = new CombatEvent.DoT
                        {
                            Value = (ushort)(param2 & ushort.MaxValue),
                            Flags2 = param2 > ushort.MaxValue ? (byte)64 : (byte)0,
                            Multiplier = (byte)(param2 / 0x10000),

                            EffectTargetId = actorId,
                            StatusId = param1,
                            EffectSourceId = param3,
                        },
                    });
                break;
            case ActorControlCategory.Death:
                this.CombatEvent?.Invoke(
                    this,
                    new CombatEvent.Death
                    {
                        ActorId = actorId,
                        SourceId = param1,
                    });
                break;
            case ActorControlCategory.GainEffect: // gain status effect, seen param3=param4=0
                if (this.Debug)
                {
                    this.Log($"[Network] -- gained {this.utils.StatusString(param1)}, extra={param2:X4}");
                }

                this.CombatEvent?.Invoke(
                    this,
                    new CombatEvent.StatusEffect
                    {
                        ActorId = actorId,
                        Grain = true,
                        StatusId = (ushort)(param1 & ushort.MaxValue),
                    });

                break;
            case ActorControlCategory.LoseEffect: // lose status effect, seen param2=param4=0, param3=invalid-oid
                if (this.Debug)
                {
                    this.Log($"[Network] -- lost {this.utils.StatusString(param1)}");
                }

                this.CombatEvent?.Invoke(
                    this,
                    new CombatEvent.StatusEffect
                    {
                        ActorId = actorId,
                        Grain = false,
                        StatusId = (ushort)(param1 & ushort.MaxValue),
                        SourceId = param3,
                    });
                break;
        }
    }
}