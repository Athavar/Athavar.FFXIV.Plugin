// <copyright file="DutyManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Models.Duty;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Lumina.Excel.GeneratedSheets;
using Lumina.Extensions;

public class DutyManager : IDisposable, IDutyManager
{
    private readonly IDutyState dutyState;
    private readonly IDataManager dataManager;
    private readonly IClientState clientState;
    private readonly IPluginLogger logger;

    private readonly Hook<CfPopDelegate> cfPopHook;

    private CfPopData? lastCfPopData;
    private DateTimeOffset? dutyStartTime;
    private bool dutyStarted;
    private DutyInfo? currentDutyInfo;
    private int wipeCount;

    public DutyManager(IDalamudServices dalamudServices, AddressResolver addressResolver)
    {
        this.dutyState = dalamudServices.DutyState;
        this.dataManager = dalamudServices.DataManager;
        this.clientState = dalamudServices.ClientState;
        this.logger = dalamudServices.PluginLogger;

        this.dutyState.DutyStarted += this.OnDutyStarted;
        this.dutyState.DutyWiped += this.OnDutyWiped;
        this.dutyState.DutyRecommenced += this.OnDutyRecommenced;
        this.dutyState.DutyCompleted += this.OnDutyCompleted;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
        this.clientState.CfPop += condition => dalamudServices.PluginLogger.Debug("CfPop: ContentFinderCondition id:{0}", condition.RowId);
        this.cfPopHook = dalamudServices.GameInteropProvider.HookFromAddress<CfPopDelegate>(addressResolver.CfPopPacketHandler, this.CfPopDetour);
        this.cfPopHook.Enable();

        // TODO: Handle game crash cases. Restore "lastCfPopData", "dutyStartTime", "dutyStarted"
    }

    private delegate nint CfPopDelegate(nint packetData);

    public event IDutyManager.DutyStartedDelegate? DutyStarted;

    public event IDutyManager.DutyWipedDelegate? DutyWiped;

    public event IDutyManager.DutyRecommencedDelegate? DutyRecommenced;

    public event IDutyManager.DutyEndedDelegate? DutyEnded;

    /// <inheritdoc/>
    public void Dispose()
    {
        this.cfPopHook.Disable();
        this.cfPopHook.Dispose();

        this.dutyState.DutyStarted -= this.OnDutyStarted;
        this.dutyState.DutyWiped -= this.OnDutyWiped;
        this.dutyState.DutyRecommenced -= this.OnDutyRecommenced;
        this.dutyState.DutyCompleted -= this.OnDutyCompleted;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
    }

    private unsafe nint CfPopDetour(nint packetData)
    {
        var result = this.cfPopHook.OriginalDisposeSafe(packetData);

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

    private void OnDutyStarted(object? sender, ushort territory) => this.StartDuty(territory);

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

    private void StartDuty(ushort territory)
    {
        if (this.dutyStarted)
        {
            return;
        }

        if (this.lastCfPopData is not { } cfPopData || this.CreateDutyInfo(territory, cfPopData) is not { } dutyInfo)
        {
            return;
        }

        this.currentDutyInfo = dutyInfo;

        this.dutyStartTime = DateTimeOffset.UtcNow;
        this.dutyStarted = true;
        var eventArgs = new DutyStartedEventArgs { DutyInfo = this.currentDutyInfo, StartTime = this.dutyStartTime.Value };

        this.DutyStarted?.Invoke(eventArgs);
    }

    private DutyInfo? CreateDutyInfo(ushort territoryTypeId, CfPopData cfPopData)
    {
        var territoryType = this.dataManager.Excel.GetSheet<TerritoryType>()?.GetRow(territoryTypeId);
        if (territoryType is null)
        {
            return null;
        }

        var territoryIntendedUse = territoryType.GetTerritoryIntendedUse();
        if (!territoryIntendedUse.IsDuty())
        {
            // exit, no duty
#if DEBUG
            if (!Enum.IsDefined(typeof(TerritoryIntendedUse), territoryType.TerritoryIntendedUse))
            {
                // TODO: find undefined value.
            }
#endif
            return null;
        }

        var contentRoulette = this.dataManager.Excel.GetSheet<ContentRoulette>()?.GetRow(cfPopData.ContentRouletteId) ?? throw new AthavarPluginException("Entry in ContentRoulette not found");

        return new DutyInfo { TerritoryType = territoryType, ContentRoulette = contentRoulette, ActiveContentCondition = cfPopData.GetActiveContentCondition(), JoinInProgress = cfPopData.JoinInProgress, QueuePlayerCount = cfPopData.PlayerCount };
    }

    private void EndDuty(bool completed)
    {
        if (this.dutyStarted && this.dutyStartTime is not null && this.currentDutyInfo is not null)
        {
            var start = this.dutyStartTime.Value;
            var end = DateTimeOffset.UtcNow;
            var duration = end - start;

            this.DutyEnded?.Invoke(new DutyEndedEventArgs { Completed = completed, StartTime = start, EndTime = end, Duration = duration, DutyInfo = this.currentDutyInfo, Wipes = this.wipeCount });
        }

        this.lastCfPopData = null;
        this.dutyStarted = false;
        this.dutyStartTime = null;
        this.currentDutyInfo = null;
        this.wipeCount = 0;
    }
}