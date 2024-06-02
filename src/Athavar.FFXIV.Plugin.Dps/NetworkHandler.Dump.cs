// <copyright file="NetworkHandler.Dump.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using System.Numerics;
using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.Data.ActionEffect;
using Athavar.FFXIV.Plugin.Dps.Data.Protocol;
using FFXIVClientStructs.FFXIV.Client.Game;
using Machina.FFXIV.Headers;
using Server_EffectResult = Athavar.FFXIV.Plugin.Dps.Data.Protocol.Server_EffectResult;

internal sealed partial class NetworkHandler
{
    private enum ActorControlCategory : ushort
    {
        ToggleWeapon = 0,   // from dissector
        AutoAttack = 1,     // from dissector
        SetStatus = 2,      // from dissector
        CastStart = 3,      // from dissector
        ToggleAggro = 4,    // from dissector
        ClassJobChange = 5, // from dissector
        Death = 6,          // dissector calls it DefeatMsg
        GainExpMsg = 7,     // from dissector
        LevelUpEffect = 10, // from dissector
        ExpChainMsg = 12,   // from dissector
        HpSetStat = 13,     // from dissector
        DeathAnimation = 14, // from dissector
        CancelCast = 15,    // dissector calls it CastInterrupt (ActorControl), machina calls it CancelAbility
        RecastDetails = 16, // p1=group id, p2=elapsed, p3=total
        Cooldown = 17,      // dissector calls it ActionStart (ActorControlSelf)
        GainEffect = 20,    // note: this packet only causes log message and hit vfx to appear, it does not actually update statuses
        LoseEffect = 21,
        UpdateEffect = 22,
        HoT_DoT = 23, // dissector calls it HPFloatingText
        UpdateRestedExp = 24, // from dissector
        Flee = 27,          // from dissector
        UnkVisControl = 30, // visibility control ??? (ActorControl, params=delay-after-spawn, visible, id, 0)
        TargetIcon = 34,    // dissector calls it CombatIndicationShow, this is for boss-related markers, param1 = marker id, param2=param3=param4=0
        Tether = 35,
        SpawnEffect = 37,     // from dissector
        ToggleInvisible = 38, // from dissector
        ToggleActionUnlock = 41, // from dissector
        UpdateUiExp = 43, // from dissector
        DmgTakenMsg = 45, // from dissector
        TetherCancel = 47,
        SetTarget = 50,         // from dissector
        Targetable = 54,        // dissector calls it ToggleNameHidden
        SetAnimationState = 62, // example - ASSN beacon activation; param1 = animation set index (0 or 1), param2 = animation index (0-7)
        SetModelState = 63,     // example - TEA liquid hand (open/closed); param1=ModelState row index, rest unused
        LimitBreakStart = 71,   // from dissector
        LimitBreakPartyStart = 72, // from dissector
        BubbleText = 73,        // from dissector
        DamageEffect = 80,      // from dissector
        RaiseAnimation = 81,    // from dissector
        TreasureScreenMsg = 87, // from dissector
        SetOwnerId = 89,        // from dissector
        ItemRepairMsg = 92,     // from dissector
        BluActionLearn = 99,    // from dissector
        DirectorInit = 100,     // from dissector
        DirectorClear = 101,    // from dissector
        LeveStartAnim = 102,    // from dissector
        LeveStartError = 103,   // from dissector
        DirectorEObjMod = 106,  // from dissector
        DirectorUpdate = 109,
        ItemObtainMsg = 117,          // from dissector
        DutyQuestScreenMsg = 123,     // from dissector
        FatePosition = 125,           // from dissector
        ItemObtainIcon = 132,         // from dissector
        FateItemFailMsg = 133,        // from dissector
        FateFailMsg = 134,            // from dissector
        ActionLearnMsg1 = 135,        // from dissector
        FreeEventPos = 138,           // from dissector
        FateSync = 139,               // from dissector
        DailyQuestSeed = 144,         // from dissector
        SetBGM = 161,                 // from dissector
        UnlockAetherCurrentMsg = 164, // from dissector
        RemoveName = 168,             // from dissector
        ScreenFadeOut = 170,          // from dissector
        ZoneIn = 200,                 // from dissector
        ZoneInDefaultPos = 201,       // from dissector
        TeleportStart = 203,          // from dissector
        TeleportDone = 205,           // from dissector
        TeleportDoneFadeOut = 206,    // from dissector
        DespawnZoneScreenMsg = 207,   // from dissector
        InstanceSelectDlg = 210,      // from dissector
        ActorDespawnEffect = 212,     // from dissector
        CompanionUnlock = 253,        // from dissector
        ObtainBarding = 254,          // from dissector
        EquipBarding = 255,           // from dissector
        CompanionMsg1 = 258,          // from dissector
        CompanionMsg2 = 259,          // from dissector
        ShowPetHotbar = 260,          // from dissector
        ActionLearnMsg = 265,         // from dissector
        ActorFadeOut = 266,           // from dissector
        ActorFadeIn = 267,            // from dissector
        WithdrawMsg = 268,            // from dissector
        OrderCompanion = 269,         // from dissector
        ToggleCompanion = 270,        // from dissector
        LearnCompanion = 271,         // from dissector
        ActorFateOut1 = 272,          // from dissector
        Emote = 290,                  // from dissector
        EmoteInterrupt = 291,         // from dissector
        SetPose = 295,                // from dissector
        FishingLightChange = 300,     // from dissector
        GatheringSenseMsg = 304,      // from dissector
        PartyMsg = 305,               // from dissector
        GatheringSenseMsg1 = 306,     // from dissector
        GatheringSenseMsg2 = 312,     // from dissector
        FishingMsg = 320,             // from dissector
        FishingTotalFishCaught = 322, // from dissector
        FishingBaitMsg = 325,         // from dissector
        FishingReachMsg = 327,        // from dissector
        FishingFailMsg = 328,         // from dissector
        WeeklyIntervalUpdateTime = 336, // from dissector
        MateriaConvertMsg = 350,  // from dissector
        MeldSuccessMsg = 351,     // from dissector
        MeldFailMsg = 352,        // from dissector
        MeldModeToggle = 353,     // from dissector
        AetherRestoreMsg = 355,   // from dissector
        DyeMsg = 360,             // from dissector
        ToggleCrestMsg = 362,     // from dissector
        ToggleBulkCrestMsg = 363, // from dissector
        MateriaRemoveMsg = 364,   // from dissector
        GlamourCastMsg = 365,     // from dissector
        GlamourRemoveMsg = 366,   // from dissector
        RelicInfuseMsg = 377,     // from dissector
        PlayerCurrency = 378,     // from dissector
        AetherReductionDlg = 381, // from dissector
        PlayActionTimeline = 407, // seems to be equivalent to 412?..
        EObjSetState = 409,       // from dissector
        Unk6 = 412,               // from dissector
        EObjAnimation = 413,      // from dissector
        SetTitle = 500,           // from dissector
        SetTargetSign = 502,
        SetStatusIcon = 504,             // from dissector
        LimitBreakGauge = 505,           // name from dissector
        SetHomepoint = 507,              // from dissector
        SetFavorite = 508,               // from dissector
        LearnTeleport = 509,             // from dissector
        OpenRecommendationGuide = 512,   // from dissector
        ArmoryErrorMsg = 513,            // from dissector
        AchievementPopup = 515,          // from dissector
        LogMsg = 517,                    // from dissector
        AchievementMsg = 518,            // from dissector
        SetItemLevel = 521,              // from dissector
        ChallengeEntryCompleteMsg = 523, // from dissector
        ChallengeEntryUnlockMsg = 524,   // from dissector
        DesynthOrReductionResult = 527,  // from dissector
        GilTrailMsg = 529,               // from dissector
        HuntingLogRankUnlock = 541,      // from dissector
        HuntingLogEntryUpdate = 542,     // from dissector
        HuntingLogSectionFinish = 543,   // from dissector
        HuntingLogRankFinish = 544,      // from dissector
        SetMaxGearSets = 560,            // from dissector
        SetCharaGearParamUI = 608,       // from dissector
        ToggleWireframeRendering = 609,  // from dissector
        ActionRejected = 700,            // from XivAlexander (ActorControlSelf)
        ExamineError = 703,              // from dissector
        GearSetEquipMsg = 801,           // from dissector
        SetFestival = 902,               // from dissector
        ToggleOrchestrionUnlock = 918,   // from dissector
        SetMountSpeed = 927,             // from dissector
        Dismount = 929,                  // from dissector
        BeginReplayAck = 930,            // from dissector
        EndReplayAck = 931,              // from dissector
        ShowBuildPresetUI = 1001,        // from dissector
        ShowEstateExternalAppearanceUI = 1002, // from dissector
        ShowEstateInternalAppearanceUI = 1003, // from dissector
        BuildPresetResponse = 1005,        // from dissector
        RemoveExteriorHousingItem = 1007,  // from dissector
        RemoveInteriorHousingItem = 1009,  // from dissector
        ShowHousingItemUI = 1015,          // from dissector
        HousingItemMoveConfirm = 1017,     // from dissector
        OpenEstateSettingsUI = 1023,       // from dissector
        HideAdditionalChambersDoor = 1024, // from dissector
        HousingStoreroomStatus = 1049,     // from dissector
        TripleTriadCard = 1204,            // from dissector
        TripleTriadUnknown = 1205,         // from dissector
        FateNpc = 2351,                    // from dissector
        FateInit = 2353,                   // from dissector
        FateStart = 2357,                  // from dissector
        FateEnd = 2358,                    // from dissector
        FateProgress = 2366,               // from dissector
        SetPvPState = 1504,                // from dissector
        EndDuelSession = 1505,             // from dissector
        StartDuelCountdown = 1506,         // from dissector
        StartDuel = 1507,                  // from dissector
        DuelResultScreen = 1508,           // from dissector
        SetDutyActionId = 1512,            // from dissector
        SetDutyActionHud = 1513,           // from dissector
        SetDutyActionActive = 1514,        // from dissector
        SetDutyActionRemaining = 1515,     // from dissector
        IncrementRecast = 1536,            // p1=cooldown group, p2=delta time quantized to 100ms; example is brd mage ballad proc
        EurekaStep = 1850,                 // from dissector
    }

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

    private unsafe void DumpActionEffect(Server_ActionEffectHeader* data, ActionEffect* effects, ulong* targetIDs, uint maxTargets, Vector3 targetPos)
    {
        var aid = (uint)(data->actionId - this.unkDelta);
        this.Log($"[Network] - AID={new ActionId((ActionType)data->effectDisplayType, aid)} (real={data->actionId}, anim={data->actionAnimationId}), animTarget={this.utils.ObjectString(data->animationTargetId)}, animLock={data->animationLockTime:f2}, seq={data->hiddenAnimation}, cntr={data->globalEffectCounter}, rot={data->rotation}, pos={this.utils.Vec3String(targetPos)}, var={data->variation}, someTarget={this.utils.ObjectString(data->SomeTargetID)}, u={data->unknown20:X2} {data->padding21:X4}");
        var targets = Math.Min(data->effectCount, maxTargets);
        for (var i = 0; i < targets; ++i)
        {
            var targetId = targetIDs[i];
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
                this.Log($"[Network] -- eff #{eff->EffectIndex}: id={this.utils.StatusString(eff->EffectID)}, extra={eff->unknown1:X2}, dur={eff->duration:f2}, src={this.utils.ObjectString(eff->SourceActorID)}, pad={eff->unknown1:X2} {eff->unknown3:X4}");
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