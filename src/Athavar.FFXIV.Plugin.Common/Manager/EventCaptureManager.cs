// <copyright file="EventCaptureManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Models;
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

    public delegate void ActorDeathEventDelegation(IGameObject actor, IGameObject? causeActor);

    public delegate void ActorControlEventDelegation(ActorControlCategory category, IGameObject actor, uint param1, uint param2, uint param3, uint param4);

    private delegate void ActorControlSelfDelegate(uint entityId, uint type, uint a3, uint a4, uint a5, uint a6, uint a7, uint a8, ulong a9, byte flag);

    public event ActorControlEventDelegation? ActorControlEvent;

    public event ActorDeathEventDelegation? ActorDeath;

    public void Dispose()
    {
        this.actorControlSelfHook?.Disable();
        this.actorControlSelfHook?.Dispose();
    }

    private void OnActorControl(uint entityId, uint type, uint param1, uint param2, uint param3, uint param4, uint a7, uint a8, ulong a9, byte flag)
    {
        this.actorControlSelfHook!.OriginalDisposeSafe(entityId, type, param1, param2, param3, param4, a7, a8, a9, flag);
        var entity = this.objectTable.SearchById(entityId);
        if (entity is null)
        {
            return;
        }

        switch ((ActorControlCategory)type)
        {
            case ActorControlCategory.Death:
                // a3 is actorId that has cause the death.
                var causeEntity = this.objectTable.SearchById(param1);
                this.ActorDeath?.Invoke(entity, causeEntity);
                break;
        }

        this.ActorControlEvent?.Invoke((ActorControlCategory)type, entity, param1, param2, param3, param4);
    }
}