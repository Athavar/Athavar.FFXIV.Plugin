// <copyright file="NetworkHandler.Dump.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;
using Athavar.FFXIV.Plugin.Dps.Data.Protocol;
using FFXIVClientStructs.FFXIV.Client.Game;
using Machina.FFXIV.Headers;
using Server_EffectResult = Athavar.FFXIV.Plugin.Dps.Data.Protocol.Server_EffectResult;

internal sealed partial class NetworkHandler
{
    private unsafe void DumpServerMessage(nint dataPtr, ushort opCode, uint targetActorId)
    {
        var header = (Server_MessageHeader*)dataPtr;
        this.Log($"[Network] Server message {(Opcode)opCode} -> {this.utils.ObjectString(targetActorId)} (seq={header->Seconds}): {((ulong*)dataPtr)[0]:X16} {((ulong*)dataPtr)[1]:X16}...");
        switch ((Opcode)opCode)
        {
            case Opcode.ActorControlTarget:
            {
                var p = (Server_ActorControlTarget*)dataPtr;
                this.Log($"[Network] - cat={p->category}, target={this.utils.ObjectString(p->TargetID)}, params={p->param1:X8} {p->param2:X8} {p->param3:X8} {p->param4:X8} {p->padding2:X8}, unk={p->padding:X4} {p->padding1:X8}");
                break;
            }

            /*case Opcode.ActorMove:
            //    {
            //        var p = (Server_ActorMove*)dataPtr;
            //        Log($"[Network] - {Utils.Vec3String(IntToFloatCoords(p->X, p->Y, p->Z))}, {IntToFloatAngle(p->Rotation)}, anim={p->AnimationFlags:X4}/{p->AnimationSpeed}, u={p->UnknownRotation:X2} {p->Unknown:X8}");
            //        break;
            //    }*/

            case Opcode.PlaceWaymark:
            {
                var p = (Server_Waymark*)dataPtr;
                this.Log($"[Network] - {p->Waymark}: {p->Status} at {p->PosX / 1000.0f:f3} {p->PosY / 1000.0f:f3} {p->PosZ / 1000.0f:f3}");
                break;
            }
        }
    }

    private void Log(string messageTemplate, params object[] values) => this.logger.Information(messageTemplate, values);

    private unsafe void DumpActionEffect(ActionEffectHeader* data, ActionEffect* effects, ulong* targetIDs, byte maxTargets)
    {
        var aid = (uint)(data->ActionId - this.unkDelta);
        this.Log($"[Network] - AID={new ActionId((ActionType)data->EffectDisplayType, aid)} (real={data->ActionId}, anim={data->ActionAnimationId}), animTarget={this.utils.ObjectString(data->AnimationTargetId)}, animLock={data->AnimationLockTime:f2}, seq={data->HiddenAnimation}, cntr={data->GlobalEffectCounter}, rot={data->Rotation}, var={data->Variation}, someTarget={this.utils.ObjectString(data->Unknown2)}, u={data->Unknown3:X2} {data->Unknown4:X4} {maxTargets}");
        var targets = Math.Min(data->EffectCount, maxTargets);
        for (var i = 0; i < targets; ++i)
        {
            var targetId = targetIDs[i] & uint.MaxValue;
            if (targetId == 0)
            {
                continue;
            }

            this.Log($"[Network] -- target {i} == {this.utils.ObjectString(targetId)}");
            for (var j = 0; j < 8; ++j)
            {
                var eff = effects + (i * 8) + j;
                if (eff->Type == ActionEffectType.Nothing)
                {
                    continue;
                }

                this.Log($"[Network] --- effect {j} == {eff->Type}, params={eff->Param0:X2} {eff->Param1:X2} {eff->Param2:X2} {eff->Param3:X2} {eff->Param4:X2} {eff->Value:X4}");
            }
        }
    }

    private unsafe void DumpEffectResult(int count, Server_EffectResult* entries)
    {
        var p = entries;
        for (var i = 0; i < count; ++i)
        {
            this.Log($"[Network] - [{i}] seq={p->RelatedActionSequence}/{p->RelatedActionSequence}, actor={this.utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}/{p->MaxHP}, class={p->ClassJob} mp={p->CurrentMP}, shield={p->DamageShield}");
            var cnt = Math.Min(4, (int)p->EffectCount);
            var eff = (Server_EffectResultEntry*)p->Effects;
            for (var j = 0; j < cnt; ++j)
            {
                var effectId = eff->EffectID;
                if (effectId <= 0)
                {
                    continue;
                }

                this.Log($"[Network] -- eff #{eff->EffectIndex}: id={this.utils.StatusString(effectId)}, extra={eff->unknown1:X2}, dur={eff->duration:f2}, src={this.utils.ObjectString(eff->SourceActorID)}, pad={eff->unknown1:X2} {eff->unknown3:X4}");
                ++eff;
            }

            ++p;
        }
    }

    private unsafe void DumpEffectResultBasic(int count, Server_EffectResultBasicEntry* entries)
    {
        var p = entries;
        for (var i = 0; i < count; ++i)
        {
            this.Log($"[Network] - [{i}] seq={p->RelatedActionSequence}/{p->RelatedTargetIndex}, actor={this.utils.ObjectString(p->ActorID)}, hp={p->CurrentHP}");
        }
    }
}