// <copyright file="EncounterManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Common.Definitions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

internal partial class EncounterManager : IDisposable
{
    private readonly IDalamudServices services;
    private readonly ObjectTable objectTable;
    private readonly NetworkHandler networkHandler;
    private readonly ICommandInterface ci;
    private readonly Utils utils;
    private readonly DpsConfiguration configuration;

    private readonly uint[] damageDealtHealProcs;
    private readonly uint[] damageReceivedHealProcs;
    private readonly uint[] healCastHealProcs;

    private readonly uint[] damageReceivedProcs;
    private readonly RollingList<Encounter> encounterHistory = new(20, true);

    private DateTime nextUpdate = DateTime.MinValue;

    public EncounterManager(IDalamudServices services, NetworkHandler networkHandler, IDefinitionManager definitions, Utils utils, ICommandInterface ci, Configuration configuration)
    {
        this.services = services;
        this.networkHandler = networkHandler;
        this.utils = utils;
        this.ci = ci;
        this.configuration = configuration.Dps!;

        this.objectTable = services.ObjectTable;
        Encounter.objectTable = services.ObjectTable;

        this.damageDealtHealProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.HealOnDamageDealt);
        this.damageReceivedHealProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.HealOnDamageReceived);
        this.healCastHealProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.HealOnHealCast);
        this.damageReceivedProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.DamageOnDamageReceived);

        networkHandler.CombatEvent += this.OnCombatEvent;
        services.Framework.Update += this.Update;
    }

    public RollingList<string> Log { get; } = new(100, true);

    public IReadOnlyList<Encounter> EncounterHistory => this.encounterHistory;

    public Encounter? CurrentEncounter { get; private set; } = new();

    public void Dispose()
    {
        this.services.Framework.Update -= this.Update;
        this.networkHandler.CombatEvent -= this.OnCombatEvent;
    }

    public Encounter? GetEncounter(int index = -1)
    {
        if (index >= 0 && index < this.encounterHistory.Count)
        {
            return this.encounterHistory[index];
        }

        if (this.CurrentEncounter?.IsValid() == true)
        {
            return this.CurrentEncounter;
        }

        return this.encounterHistory.Count > 0 ? this.encounterHistory[0] : null;
    }

    public void EndEncounter(bool inValid = false)
    {
        if (this.CurrentEncounter is null)
        {
            return;
        }

        if (!inValid && this.CurrentEncounter.IsValid())
        {
            this.CurrentEncounter.End = this.CurrentEncounter.LastEvent;
            this.encounterHistory.Add(this.CurrentEncounter);
        }

        this.CurrentEncounter = new Encounter();
    }

    public void Clear()
    {
        this.encounterHistory.Clear();
        this.CurrentEncounter = new Encounter();
    }

    [MemberNotNull(nameof(CurrentEncounter))]
    private void StartEncounter(DateTime? time = null)
    {
        if (this.CurrentEncounter is not null)
        {
            this.EndEncounter();
        }

        var territory = this.services.ClientState.TerritoryType;
        var territoryName = this.services.DataManager.GetExcelSheet<TerritoryType>()?.GetRow(territory)?.PlaceName.Value?.Name.ToDalamudString();
        var start = time ?? DateTime.Now;
        this.CurrentEncounter = new Encounter(territoryName?.ToString() ?? string.Empty, territory)
        {
            Start = start,
            LastEvent = start,
        };
    }

    private void Update(Framework framework)
    {
        var now = DateTime.UtcNow;
        if (now < this.nextUpdate)
        {
            return;
        }

        this.nextUpdate = now.AddSeconds(2);

        var ce = this.CurrentEncounter;
        if (ce is null || ce.Start == DateTime.MinValue)
        {
            return;
        }

        if ((!this.ci.IsInCombat() && ce.LastEvent.AddSeconds(15) < now) || ce.Territory != this.ci.GetCurrentTerritory())
        {
            this.EndEncounter();
        }

        ce.UpdateParty(this.configuration, this.services);

        if (!ce.IsValid())
        {
            this.EndEncounter(true);
        }
    }
}