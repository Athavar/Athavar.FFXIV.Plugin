// <copyright file="EncounterManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Common.Definitions;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using Action = Lumina.Excel.Sheets.Action;
using IDefinitionManager = Athavar.FFXIV.Plugin.Common.Manager.Interface.IDefinitionManager;

internal sealed partial class EncounterManager : IDisposable
{
    private readonly IDalamudServices services;
    private readonly IObjectTable objectTable;
    private readonly NetworkHandler networkHandler;
    private readonly IDefinitionManager definitions;
    private readonly ICommandInterface ci;
    private readonly Utils utils;
    private readonly DpsConfiguration configuration;
    private readonly IFrameworkManager frameworkManager;
    private readonly IPluginLogger logger;

    private readonly uint[] damageDealtHealProcs;
    private readonly uint[] damageReceivedHealProcs;
    private readonly uint[] healCastHealProcs;

    private readonly uint[] damageReceivedProcs;
    private readonly uint[] limitBreaks;
    private readonly RollingList<TerritoryEncounter> encounterHistory = new(20, true);

    private DateTime nextUpdate = DateTime.MinValue;
    private DateTime nextStatUpdate = DateTime.MinValue;

    public EncounterManager(IDalamudServices services, NetworkHandler networkHandler, IDefinitionManager definitions, Utils utils, ICommandInterface ci, DpsConfiguration configuration, IFrameworkManager frameworkManager, IPluginLogger logger)
    {
        this.services = services;
        this.networkHandler = networkHandler;
        this.definitions = definitions;
        this.utils = utils;
        this.ci = ci;
        this.configuration = configuration;
        this.frameworkManager = frameworkManager;
        this.logger = logger;

        this.objectTable = services.ObjectTable;
        Encounter.ObjectTable = services.ObjectTable;

        this.limitBreaks = services.DataManager.GetExcelSheet<Action>()?.Where(a => a.ActionCategory.RowId == 9).Select(a => a.RowId).ToArray() ?? throw new AthavarPluginException();

        this.damageDealtHealProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.HealOnDamageDealt);
        this.damageReceivedHealProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.HealOnDamageReceived);
        this.healCastHealProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.HealOnHealCast);
        this.damageReceivedProcs = definitions.GetStatusIdsByReactiveProcType(ReactiveProc.ReactiveProcType.DamageOnDamageReceived);

        networkHandler.CombatEvent += this.OnCombatEvent;
        frameworkManager.Subscribe(this.Update);
    }

    public List<string> Log { get; } = new(); // new(100, true);

    public IReadOnlyList<TerritoryEncounter> EncounterHistory => this.encounterHistory;

    public Encounter? CurrentEncounter { get; private set; } = new();

    public TerritoryEncounter? CurrentTerritoryEncounter { get; private set; }

    public void ResizeHistory(int max)
    {
        if (max <= 1)
        {
            throw new ArgumentException(null, nameof(max));
        }

        this.encounterHistory.Resize(max);
    }

    public void Dispose()
    {
        this.frameworkManager.Unsubscribe(this.Update);
        this.networkHandler.CombatEvent -= this.OnCombatEvent;
    }

    public BaseEncounter? GetEncounter(int territoryIndex = -1, int encounterIndex = -1)
    {
        TerritoryEncounter? territory;
        var historyCount = this.encounterHistory.Count;
        if (territoryIndex >= 0 && territoryIndex < historyCount)
        {
            territory = this.encounterHistory[territoryIndex];
        }
        else if (this.CurrentEncounter?.IsValid() == true)
        {
            return this.CurrentEncounter;
        }
        else if (this.encounterHistory.Count > 0)
        {
            territory = this.encounterHistory.First();
            encounterIndex = territory.Encounters.Count - 1;
        }
        else
        {
            return null;
        }

        var encounterCount = territory.Encounters.Count;
        if (encounterIndex >= 0 && encounterIndex < encounterCount)
        {
            return territory.Encounters[encounterIndex];
        }

        return territory;
    }

    public void Clear()
    {
        this.encounterHistory.Clear();
        this.CurrentEncounter = new Encounter();
        this.CurrentTerritoryEncounter = null;
    }

    private void Update(IFramework framework)
    {
        var now = DateTime.UtcNow;

        if (this.CurrentEncounter is not { } ce || ce.Start == DateTime.MinValue)
        {
            return;
        }

        if (now >= this.nextStatUpdate)
        {
            this.nextStatUpdate = now.AddMilliseconds(100);
            ce.Filter = this.configuration.PartyFilter;
            ce.CalcStats();

            // TODO: check IPlayerCharacter is working for OfType
            if (!this.ci.IsInCombat() && ce.Start.AddSeconds(2) < now && ce.LastDamageEvent.AddSeconds(2.5) < now && !ce.AllyCombatants.Select(c => this.objectTable.SearchById(c.GameObjectId)).OfType<IPlayerCharacter>().Any(go => go.IsValid() && (go.StatusFlags & StatusFlags.InCombat) != 0))
            {
                /*
                 * - player is not in combat
                 * - start of encounter was 2 seconds ago. necessary, because combat state is not directly updated.
                 * - all ally combatants are no longer in combat.
                 */
                this.logger.Verbose("End CurrentEncounter - CombatCheck");
                this.EndEncounter();
                this.UpdateCurrentTerritoryEncounter();
                return;
            }

            if (this.CurrentTerritoryEncounter is not null && this.CurrentTerritoryEncounter.Territory != this.ci.GetCurrentTerritory())
            {
                this.logger.Verbose("End CurrentEncounter - TerritoryCheck");
                this.EndEncounter();
                this.EndCurrentTerritoryEncounter();
                return;
            }
        }

        if (now >= this.nextUpdate)
        {
            this.nextUpdate = now.AddSeconds(1);

            ce.Filter = this.configuration.PartyFilter;
            ce.CalcStats();
            ce.UpdateParty(this.services);

            if (!ce.IsValid())
            {
                this.logger.Verbose("End CurrentEncounter - CurrentInvalid");
                this.EndEncounter(true);
                return;
            }

            if (ce.TerritoryEncounter is null)
            {
                this.AddEncounterToCurrentTerritoryEncounter(ce);
            }

            this.UpdateCurrentTerritoryEncounter();
        }
    }
}