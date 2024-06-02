// <copyright file="EventCaptureManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;

public sealed class EventCaptureManager : IDisposable
{
    private readonly IPluginLogger logger;
    private readonly IObjectTable objectTable;
    private Hook<ActorControlSelfDelegate>? actorControlSelfHook;

    public EventCaptureManager(IDalamudServices dalamudServices, AddressResolver addressResolver)
    {
        this.objectTable = dalamudServices.ObjectTable;
        this.logger = dalamudServices.PluginLogger;

        dalamudServices.SafeEnableHookFromAddress<ActorControlSelfDelegate>("EventCaptureManager:actorControlSelfHook", addressResolver.ActorControlHandler, this.OnActorControl, h => this.actorControlSelfHook = h);
    }

    public delegate void ActorDeathEventDelegation(GameObject actor, GameObject? causeActor);

    private delegate void ActorControlSelfDelegate(uint entityId, uint type, uint a3, uint a4, uint a5, uint a6, uint a7, uint a8, ulong a9, byte flag);

    public event ActorDeathEventDelegation? ActorDeath;

    private enum ActorControlCategory
    {
        Death = 0x6,
        CancelAbility = 0xF,
        GainEffect = 0x14,
        LoseEffect = 0x15,
        UpdateEffect = 0x16,
        TargetIcon = 0x22,
        Tether = 0x23,
        Targetable = 0x36,
        DirectorUpdate = 0x6D,
        SetTargetSign = 0x1F6,
        LimitBreak = 0x1F9,
        HoT = 0x603,
        DoT = 0x604,
    }

    public void Dispose()
    {
        this.actorControlSelfHook?.Disable();
        this.actorControlSelfHook?.Dispose();
    }

    private void OnActorControl(uint entityId, uint type, uint a3, uint a4, uint a5, uint a6, uint a7, uint a8, ulong a9, byte flag)
    {
        this.actorControlSelfHook!.OriginalDisposeSafe(entityId, type, a3, a4, a5, a6, a7, a8, a9, flag);
        var entity = this.objectTable.SearchById(entityId);
        if (entity is null)
        {
            return;
        }

        switch ((ActorControlCategory)type)
        {
            case ActorControlCategory.Death:
                // a3 is actorId that has cause the death.
                var causeEntity = this.objectTable.SearchById(a3);
                this.ActorDeath?.Invoke(entity, causeEntity);
                break;
        }
    }
}