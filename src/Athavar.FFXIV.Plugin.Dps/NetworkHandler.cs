// <copyright file="NetworkHandler.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;
using Athavar.FFXIV.Plugin.Dps.Data.Protocol;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.Network;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using Machina.FFXIV.Headers;
using Server_ActorCast = Athavar.FFXIV.Plugin.Dps.Data.Protocol.Server_ActorCast;
using Server_EffectResult = Athavar.FFXIV.Plugin.Dps.Data.Protocol.Server_EffectResult;

internal sealed partial class NetworkHandler : IDisposable
{
    private readonly IPluginLogger logger;
    private readonly IGameNetwork gameNetwork;
    private readonly IOpcodeManager opcodeManager;
    private readonly IDefinitionManager definitionManager;
    private readonly Utils utils;

    // this is a mega weird thing - apparently some IDs sent over network have some extra delta added to them (e.g. action ids, icon ids, etc.)
    // they change on relogs or zone changes or something...
    // we have one simple way of detecting them - by looking at casts, since they contain both offset id and real ('animation') id
    private long unkDelta;

    public NetworkHandler(IDalamudServices dalamudServices, IOpcodeManager opcodeManager, Utils utils, IDefinitionManager definitionManager)
    {
        this.logger = dalamudServices.PluginLogger;
        this.gameNetwork = dalamudServices.GameNetwork;
        this.opcodeManager = opcodeManager;
        this.utils = utils;
        this.definitionManager = definitionManager;

        this.gameNetwork.NetworkMessage += this.HandleMessage;
    }

    public event EventHandler<(ulong ActorID, uint Seq, int TargetIndex)>? EventEffectResult;

    public event EventHandler<(ulong ActorID, ActionId Action, float CastTime, ulong TargetID)>? EventActorCast;

    public event EventHandler<(ulong ActorID, uint ActionID)>? EventActorControlCancelCast;

    public event EventHandler<(ulong ActorID, uint IconID)>? EventActorControlTargetIcon;

    public event EventHandler<(ulong ActorId, ulong TargetID, uint TetherID)>? EventActorControlTether;

    public event EventHandler<CombatEvent>? CombatEvent;

    public bool Debug { get; set; } = false;

    public bool Enable { get; set; } = true;

    public void Dispose() => this.gameNetwork.NetworkMessage -= this.HandleMessage;

    private static Vector3 IntToFloatCoords(uint xy, ushort z) => IntToFloatCoords((ushort)(xy >> 8), (ushort)(xy & 0xF), z);

    private static Vector3 IntToFloatCoords(ushort x, ushort y, ushort z)
    {
        var fx = (x * (2000.0f / 65535)) - 1000;
        var fy = (y * (2000.0f / 65535)) - 1000;
        var fz = (z * (2000.0f / 65535)) - 1000;
        return new Vector3(fx, fy, fz);
    }

    private static Angle IntToFloatAngle(ushort rot) => (((rot / 65535.0f) * (2 * MathF.PI)) - MathF.PI).Radians();

    private unsafe void HandleMessage(nint dataPtr, ushort opCode, uint sourceActorId, uint targetActorId, NetworkMessageDirection direction)
    {
        if (!this.Enable)
        {
            return;
        }

        const int headerSize = 0x20;
        if (direction != NetworkMessageDirection.ZoneDown)
        {
            // client->server
            return;
        }

        if (!this.opcodeManager.Opcodes.TryGetValue(opCode, out var opcodeType))
        {
            return;
        }

        dataPtr -= headerSize;
        if (this.Debug)
        {
            this.DumpServerMessage(dataPtr, opCode, targetActorId);
        }

        // server->client
        switch (opcodeType)
        {
            case Opcode.ActionEffect1:
                this.HandleActionEffect1((Server_ActionEffect1*)dataPtr, targetActorId);
                break;
            case Opcode.ActionEffect8:
                this.HandleActionEffect8((Server_ActionEffect8*)dataPtr, targetActorId);
                break;
            case Opcode.ActionEffect16:
                this.HandleActionEffect16((Server_ActionEffect16*)dataPtr, targetActorId);
                break;
            case Opcode.ActionEffect24:
                this.HandleActionEffect24((Server_ActionEffect24*)dataPtr, targetActorId);
                break;
            case Opcode.ActionEffect32:
                this.HandleActionEffect32((Server_ActionEffect32*)dataPtr, targetActorId);
                break;
            case Opcode.EffectResultBasic:
                this.HandleEffectResultBasic(Math.Min((byte)1, *(byte*)(dataPtr + headerSize)), (Server_EffectResultBasicEntry*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResultBasic4:
                this.HandleEffectResultBasic(Math.Min((byte)4, *(byte*)(dataPtr + headerSize)), (Server_EffectResultBasicEntry*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResultBasic8:
                this.HandleEffectResultBasic(Math.Min((byte)8, *(byte*)(dataPtr + headerSize)), (Server_EffectResultBasicEntry*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResultBasic16:
                this.HandleEffectResultBasic(Math.Min((byte)16, *(byte*)(dataPtr + headerSize)), (Server_EffectResultBasicEntry*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResultBasic32:
                this.HandleEffectResultBasic(Math.Min((byte)32, *(byte*)(dataPtr + headerSize)), (Server_EffectResultBasicEntry*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResultBasic64:
                this.HandleEffectResultBasic(Math.Min((byte)64, *(byte*)(dataPtr + headerSize)), (Server_EffectResultBasicEntry*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResult:
                this.HandleEffectResult(Math.Min((byte)1, *(byte*)(dataPtr + headerSize)), (Server_EffectResult*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResult4:
                this.HandleEffectResult(Math.Min((byte)4, *(byte*)(dataPtr + headerSize)), (Server_EffectResult*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResult8:
                this.HandleEffectResult(Math.Min((byte)8, *(byte*)(dataPtr + headerSize)), (Server_EffectResult*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.EffectResult16:
                this.HandleEffectResult(Math.Min((byte)16, *(byte*)(dataPtr + headerSize)), (Server_EffectResult*)(dataPtr + headerSize + 4), targetActorId);
                break;
            case Opcode.ActorCast:
                this.HandleActorCast((Server_ActorCast*)dataPtr, targetActorId);
                break;
            case Opcode.ActorControl:
                this.HandleActorControl((Server_ActorControl*)dataPtr, targetActorId);
                break;
            case Opcode.ActorControlSelf:
                this.HandleActorControlSelf((Server_ActorControlSelf*)dataPtr, targetActorId);
                break;
        }
    }

    private unsafe void HandleActionEffect1(Server_ActionEffect1* p, uint actorId) => this.HandleActionEffect(actorId, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 1, default);

    private unsafe void HandleActionEffect8(Server_ActionEffect8* p, uint actorId) => this.HandleActionEffect(actorId, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 8, IntToFloatCoords(p->effectflags1, p->effectflags2));

    private unsafe void HandleActionEffect16(Server_ActionEffect16* p, uint actorId) => this.HandleActionEffect(actorId, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 16, IntToFloatCoords(p->effectflags1, p->effectflags2));

    private unsafe void HandleActionEffect24(Server_ActionEffect24* p, uint actorId) => this.HandleActionEffect(actorId, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 24, IntToFloatCoords(p->effectflags1, p->effectflags2));

    private unsafe void HandleActionEffect32(Server_ActionEffect32* p, uint actorId) => this.HandleActionEffect(actorId, &p->Header, (ActionEffect*)p->Effects, p->TargetID, 32, IntToFloatCoords(p->effectflags1, p->effectflags2));

    private unsafe void HandleActionEffect(uint casterId, Server_ActionEffectHeader* header, ActionEffect* effects, ulong* targetIds, uint maxTargets, Vector3 targetPos)
    {
        if (this.Debug)
        {
            this.DumpActionEffect(header, effects, targetIds, maxTargets, targetPos);
        }

        if ((byte)header->effectDisplayType == (byte)ActionType.Action)
        {
            var newDelta = (int)header->actionId - header->actionAnimationId;
            if (this.unkDelta != newDelta)
            {
                this.logger.Verbose($"Updating network delta: {this.unkDelta} -> {newDelta}");
                this.unkDelta = newDelta;
            }
        }

        var actionType = (byte)header->effectDisplayType != (byte)ActionType.Mount
            ? (byte)header->effectDisplayType != (byte)ActionType.Item ? ActionType.Action : ActionType.Item
            : ActionType.Mount;

        var actionId = actionType == ActionType.Action ? header->actionAnimationId : header->actionId;

        var actionEffects = new List<CombatEvent.ActionEffectEvent>();

        var targets = Math.Min(header->effectCount, maxTargets);
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
                        actionEffects.Add(new CombatEvent.DamageTaken
                        {
                            HitSeverity = actionEffect.Param0,
                            Param1 = actionEffect.Param1,
                            Percentage = actionEffect.Param2,
                            Multiplier = actionEffect.Param3,
                            Flags2 = actionEffect.Param4,
                            Value = actionEffect.Value,

                            EffectTargetId = targetId,
                            EffectSourceId = casterId,
                            ActionId = actionId,
                            ActionType = actionType,
                        });
                        break;
                    }

                    case ActionEffectType.Heal:
                    {
                        actionEffects.Add(new CombatEvent.Healed
                        {
                            HitSeverity = actionEffect.Param1,
                            Param1 = actionEffect.Param0,
                            Percentage = actionEffect.Param2,
                            Multiplier = actionEffect.Param3,
                            Flags2 = actionEffect.Param4,
                            Value = actionEffect.Value,

                            EffectTargetId = targetId,
                            EffectSourceId = casterId,
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
            SequenceId = header->globalEffectCounter,
            ActionId = new ActionId(actionType, actionId),
            Definition = this.definitionManager.GetActionById(actionId),
            TargetId = header->animationTargetId,
            ActorId = casterId,
            Effects = actionEffects.ToArray(),
        };

        this.CombatEvent?.Invoke(this, info);
    }

    private unsafe void HandleEffectResultBasic(int count, Server_EffectResultBasicEntry* p, uint actorId)
    {
        if (this.Debug)
        {
            this.DumpEffectResultBasic(count, p);
        }

        for (var i = 0; i < count; ++i)
        {
            this.EventEffectResult?.Invoke(this, (actorId, p->RelatedActionSequence, p->RelatedTargetIndex));
            ++p;
        }
    }

    private unsafe void HandleEffectResult(int count, Server_EffectResult* entries, uint actorId)
    {
        if (this.Debug)
        {
            this.DumpEffectResult(count, entries);
        }

        var p = entries;
        for (var i = 0; i < count; ++i)
        {
            var cnt = Math.Min(4, (int)p->EffectCount);
            var eff = (Server_EffectResultEntry*)p->Effects;
            for (var j = 0; j < cnt; ++j)
            {
                this.CombatEvent?.Invoke(this, new CombatEvent.StatusEffect
                {
                    ActorId = p->ActorID,
                    SourceId = eff->SourceActorID,
                    Grain = true,
                    StatusId = eff->EffectID,
                    Duration = eff->duration,
                });
                ++eff;
            }

            ++p;
        }
    }

    private unsafe void HandleActorCast(Server_ActorCast* p, uint actorId)
    {
        var action = new ActionId(p->ActionType, p->SpellId);
        if (this.Debug)
        {
            this.Log($"[Network] - AID={action} ({new ActionId(ActionType.Action, p->SpellId)}), target={this.utils.ObjectString(p->TargetID)}, time={p->CastTime:f2} ({p->BaseCastTime100ms * 0.1f:f1}), rot={IntToFloatAngle(p->Rotation)}, targetpos={this.utils.Vec3String(IntToFloatCoords(p->PosX, p->PosY, p->PosZ))}, interruptible={p->Interruptible}, u1={p->U1:X2}, u2={this.utils.ObjectString(p->U2ObjID)}, u3={p->U3:X4}");
        }

        this.EventActorCast?.Invoke(this, (actorId, action, p->CastTime, p->TargetID));
    }

    private unsafe void HandleActorControl(Server_ActorControl* p, uint actorId)
    {
        if (this.Debug /*|| p->category is Server_ActorControlCategory.HoT or Server_ActorControlCategory.DoT*/)
        {
            this.Log($"[Network] {this.utils.ObjectString(actorId)} - cat={p->category.AsText()}|{((ActorControlCategory)p->category).AsText()}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->padding1:X8}, unk={p->padding:X4}");
        }

        switch (p->category)
        {
            case Server_ActorControlCategory.HoT:
                this.CombatEvent?.Invoke(this, new CombatEvent.DeferredEvent
                {
                    ActorId = actorId,
                    EffectEvent = new CombatEvent.HoT
                    {
                        Value = (ushort)(p->param2 & ushort.MaxValue),
                        Flags2 = p->param2 > ushort.MaxValue ? (byte)64 : (byte)0,
                        Multiplier = (byte)(p->param2 / 0x10000),

                        EffectTargetId = actorId,
                        StatusId = p->param1,
                        EffectSourceId = p->param3,
                    },
                });
                break;
            case Server_ActorControlCategory.DoT:
                this.CombatEvent?.Invoke(this, new CombatEvent.DeferredEvent
                {
                    ActorId = actorId,
                    EffectEvent = new CombatEvent.DoT
                    {
                        Value = (ushort)(p->param2 & ushort.MaxValue),
                        Flags2 = p->param2 > ushort.MaxValue ? (byte)64 : (byte)0,
                        Multiplier = (byte)(p->param2 / 0x10000),

                        EffectTargetId = actorId,
                        StatusId = p->param1,
                        EffectSourceId = p->param3,
                    },
                });
                break;
            case Server_ActorControlCategory.CancelAbility: // note: some successful boss casts have this message on completion, seen param1=param4=0, param2=1; param1 is related to cast time?.
                if (this.Debug)
                {
                    this.Log($"[Network] -- cancelled {new ActionId((ActionType)p->param2, p->param3)}, interrupted={p->param4 == 1}");
                }

                this.EventActorControlCancelCast?.Invoke(this, (actorId, p->param3));
                break;
            case Server_ActorControlCategory.Death:
                this.CombatEvent?.Invoke(this, new CombatEvent.Death
                {
                    ActorId = actorId,
                    SourceId = p->param1,
                });
                break;
            case Server_ActorControlCategory.TargetIcon:
                this.EventActorControlTargetIcon?.Invoke(this, (actorId, (uint)(p->param1 - this.unkDelta)));
                break;
            case Server_ActorControlCategory.Tether:
                this.EventActorControlTether?.Invoke(this, (actorId, p->param3, p->param2));
                break;
            case Server_ActorControlCategory.UpdateEffect:
                if (this.Debug)
                {
                    this.Log($"[Network] -- update {this.utils.StatusString(p->param1)}");
                }

                break;
            case Server_ActorControlCategory.GainEffect: // gain status effect, seen param3=param4=0
                if (this.Debug)
                {
                    this.Log($"[Network] -- gained {this.utils.StatusString(p->param1)}, extra={p->param2:X4}");
                }

                this.CombatEvent?.Invoke(this, new CombatEvent.StatusEffect
                {
                    ActorId = actorId,
                    Grain = true,
                    StatusId = (ushort)(p->param1 & ushort.MaxValue),
                });

                break;
            case Server_ActorControlCategory.LoseEffect: // lose status effect, seen param2=param4=0, param3=invalid-oid
                if (this.Debug)
                {
                    this.Log($"[Network] -- lost {this.utils.StatusString(p->param1)}");
                }

                this.CombatEvent?.Invoke(this, new CombatEvent.StatusEffect
                {
                    ActorId = actorId,
                    Grain = false,
                    StatusId = (ushort)(p->param1 & ushort.MaxValue),
                    SourceId = p->param3,
                });
                break;
        }
    }

    private unsafe void HandleActorControlSelf(Server_ActorControlSelf* p, uint actorID)
    {
        if (this.Debug)
        {
            // this.Log($"[Network] Self - cat={p->category.AsText()}|{((ActorControlCategory)p->category).AsText()}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->param5:X8} {p->param6:X8} {p->padding1:X8}, unk={p->padding:X4}");
        }

        switch (p->category)
        {
            case Server_ActorControlCategory.DirectorUpdate:
                break;
            case Server_ActorControlCategory.LimitBreak:
                break;
        }
    }
}