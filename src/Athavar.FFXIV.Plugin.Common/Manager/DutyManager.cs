// <copyright file="DutyManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Duty;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using Lumina.Extensions;
using TerritoryIntendedUse = Athavar.FFXIV.Plugin.Models.Duty.TerritoryIntendedUse;

internal sealed partial class DutyManager : IDisposable, IDutyManager
{
    private readonly IDutyState dutyState;
    private readonly IDataManager dataManager;
    private readonly IClientState clientState;
    private readonly IPartyList partyList;
    private readonly IPluginLogger logger;
    private readonly EventCaptureManager eventCaptureManager;
    private readonly CommonConfiguration configuration;

    private Hook<CfPopDelegate>? cfPopHook;

    private CfPopData? lastCfPopData;
    private DateTimeOffset? dutyStartTime;
    private bool dutyStarted;
    private bool dirtyTracking;
    private DutyInfo? currentDutyInfo;
    private int wipeCount;
    private int playerDeathCount;

    public DutyManager(IDalamudServices dalamudServices, AddressResolver addressResolver, EventCaptureManager eventCaptureManager, CommonConfiguration configuration)
    {
        this.dutyState = dalamudServices.DutyState;
        this.dataManager = dalamudServices.DataManager;
        this.clientState = dalamudServices.ClientState;
        this.partyList = dalamudServices.PartyList;
        this.logger = dalamudServices.PluginLogger;
        this.eventCaptureManager = eventCaptureManager;
        this.configuration = configuration;

        this.dutyState.DutyStarted += this.OnDutyStarted;
        this.dutyState.DutyWiped += this.OnDutyWiped;
        this.dutyState.DutyRecommenced += this.OnDutyRecommenced;
        this.dutyState.DutyCompleted += this.OnDutyCompleted;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
        this.Restore();
        this.clientState.Login += this.OnLogin;
        this.eventCaptureManager.ActorDeath += this.OnActorDeath;

        dalamudServices.SafeEnableHookFromAddress<CfPopDelegate>("DutyManager:cfPopHook", addressResolver.CfPopPacketHandler, this.CfPopDetour, h => this.cfPopHook = h);
    }

    private delegate nint CfPopDelegate(nint packetData);

    public event IDutyManager.DutyStartedDelegate? DutyStarted;

    public event IDutyManager.DutyWipedDelegate? DutyWiped;

    public event IDutyManager.DutyRecommencedDelegate? DutyRecommenced;

    public event IDutyManager.DutyEndedDelegate? DutyEnded;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.cfPopHook?.Disable();
        this.cfPopHook?.Dispose();

        this.dutyState.DutyStarted -= this.OnDutyStarted;
        this.dutyState.DutyWiped -= this.OnDutyWiped;
        this.dutyState.DutyRecommenced -= this.OnDutyRecommenced;
        this.dutyState.DutyCompleted -= this.OnDutyCompleted;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
        this.clientState.Login -= this.OnLogin;
        this.eventCaptureManager.ActorDeath -= this.OnActorDeath;
    }

    private unsafe nint CfPopDetour(nint packetData)
    {
        var result = this.cfPopHook?.OriginalDisposeSafe(packetData) ?? nint.Zero;

        // https://github.com/goatcorp/Dalamud/blob/e30c904ad62bdcb527c72eaf6721418a23ef5078/Dalamud/Game/Network/Internal/NetworkHandlers.cs#L239
        using var stream = new UnmanagedMemoryStream((byte*)packetData, 40);
        using var reader = new BinaryReader(stream);
        var allData = reader.PeekBytes(40);
        this.lastCfPopData = reader.ReadStructure<CfPopData>();
        stream.Position = 0;

        this.logger.Verbose(this.lastCfPopData?.ToString() ?? string.Empty);
        this.logger.Verbose(BitConverter.ToString(allData));

        return result;
    }

    private void OnDutyCompleted(object? sender, ushort territory)
    {
        if (this.dutyStarted && this.dutyState.IsDutyStarted == false)
        {
            this.EndDuty(true);
        }
    }

    private void OnDutyWiped(object? sender, ushort territory)
    {
        this.wipeCount++;
        this.DutyWiped?.Invoke();
    }

    private void OnDutyRecommenced(object? o, ushort territory) => this.DutyRecommenced?.Invoke();

    private void OnDutyStarted(object? sender, ushort territory) => this.StartDuty(territory, true);

    // This gets called before DutyState.DutyCompleted, so we can intercept in case the duty is abandoned instead of completed.
    private void OnTerritoryChanged(ushort territoryTypeId)
    {
        if (this.dutyStarted && this.dutyState.IsDutyStarted == false)
        {
            this.EndDuty(false);
        }

        if (this.lastCfPopData is { JoinInProgress: true })
        {
            this.StartDuty(territoryTypeId);
        }

        if (this.lastCfPopData is not null)
        {
            var territoryType = this.dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(territoryTypeId);
            if (territoryType?.GetTerritoryIntendedUse().IsDuty() == false)
            {
                // clear data. Territory is no duty.
                this.lastCfPopData = null;
            }
        }
    }

    private void OnLogin() => this.Restore();
}

/// <summary>
///     Duty Control.
/// </summary>
internal sealed partial class DutyManager
{
    private void StartDuty(ushort territory, bool startOverride = false)
    {
        if (!startOverride && this.dutyStarted)
        {
            return;
        }

        if (this.CreateDutyInfo(territory, this.lastCfPopData) is not { } dutyInfo)
        {
            return;
        }

        this.Clear();

        this.currentDutyInfo = dutyInfo;
        this.dutyStartTime = DateTimeOffset.UtcNow;
        this.dutyStarted = true;
        var eventArgs = new DutyStartedEventArgs { DutyInfo = this.currentDutyInfo, StartTime = this.dutyStartTime.Value };
        this.Save();

        this.DutyStarted?.Invoke(eventArgs);
    }

    private DutyInfo? CreateDutyInfo(ushort territoryTypeId, CfPopData? cfPopData)
    {
        if (this.dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(territoryTypeId) is not { } territoryType)
        {
            return null;
        }

        var territoryIntendedUse = territoryType.GetTerritoryIntendedUse();
        if (!territoryIntendedUse.IsDuty())
        {
            // exit, no duty
#if DEBUG
            if (territoryType.TerritoryIntendedUse.RowId is var rowId && !Enum.IsDefined(typeof(TerritoryIntendedUse), (byte)rowId))
            {
                // TODO: find undefined value.
            }
#endif
            return null;
        }

        var playerCount = cfPopData?.PlayerCount ?? this.partyList.Length;
        if (playerCount == 0)
        {
            playerCount = 1;
        }

        var contentRoulette = this.dataManager.Excel.GetSheet<ContentRoulette>()?.GetRow(cfPopData?.ContentRouletteId ?? 0) ?? throw new AthavarPluginException("Entry in ContentRoulette not found");

        return new DutyInfo
        {
            TerritoryType = territoryType,
            ContentRoulette = contentRoulette,
            ActiveContentCondition = cfPopData?.GetActiveContentCondition() ?? 0,
            JoinInProgress = cfPopData?.JoinInProgress ?? false,
            QueuePlayerCount = playerCount,
        };
    }

    private void EndDuty(bool completed)
    {
        if (this.dutyStarted && this.dutyStartTime is not null && this.currentDutyInfo is not null)
        {
            var start = this.dutyStartTime.Value;
            var end = DateTimeOffset.UtcNow;
            var duration = end - start;

            this.DutyEnded?.Invoke(new DutyEndedEventArgs { Completed = completed, StartTime = start, EndTime = end, Duration = duration, DutyInfo = this.currentDutyInfo, Wipes = this.wipeCount, PlayerDeaths = this.playerDeathCount, TrackingWasInterrupted = this.dirtyTracking });
        }

        this.Clear();
    }
}

/// <summary>
///     Duty Config Saving.
/// </summary>
internal sealed partial class DutyManager
{
    private void Restore()
    {
        if (!this.clientState.IsLoggedIn)
        {
            return;
        }

        this.logger.Verbose("Check if configuration contains dutyInfo for restore");

        if (this.configuration.SavedDutyInfos.TryGetValue(this.clientState.LocalContentId, out var savedDutyInfo))
        {
            // we have saved duty info. set duty to started.
            this.dutyStarted = true;
            this.dirtyTracking = true;
            this.currentDutyInfo = savedDutyInfo.GetDutyInfo(this.dataManager);
            this.dutyStartTime = savedDutyInfo.DutyStartTime;
            this.logger.Verbose("Restore dutyInfo for localContentId {0}", this.clientState.LocalContentId);

            // check if player is currently in saved duty.
            if (this.currentDutyInfo is not null && this.clientState.TerritoryType != this.currentDutyInfo?.TerritoryType.RowId)
            {
                // current territory is not matching saved duty-info. End duty is clearing the saved data.
                this.EndDuty(false);
            }
        }
    }

    private void Save()
    {
        if (this.currentDutyInfo is null || this.dutyStartTime is null)
        {
            // we are currently not in a duty. clear state.
            this.Clear();
        }
        else
        {
            // save current data.
            this.configuration.SavedDutyInfos[this.clientState.LocalContentId] = new SavedDutyInfo(this.currentDutyInfo, this.dutyStartTime.Value);
            this.configuration.Save();
        }
    }

    private void Clear()
    {
        this.lastCfPopData = null;
        this.dutyStarted = false;
        this.dutyStartTime = null;
        this.currentDutyInfo = null;
        this.wipeCount = 0;
        this.playerDeathCount = 0;
        this.dirtyTracking = false;

        if (this.configuration.SavedDutyInfos.Remove(this.clientState.LocalContentId))
        {
            this.configuration.Save();
        }
    }
}

/// <summary>
///     Death Tracking.
/// </summary>
internal sealed partial class DutyManager
{
    private void OnActorDeath(IGameObject actor, IGameObject? causeActor)
    {
        if (!this.dutyStarted || actor is not IPlayerCharacter playerCharacter)
        {
            return;
        }

        this.playerDeathCount++;
    }
}