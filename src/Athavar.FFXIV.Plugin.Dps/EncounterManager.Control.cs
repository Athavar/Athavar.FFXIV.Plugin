// <copyright file="EncounterManager.Control.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

internal sealed partial class EncounterManager
{
    private void EndEncounter(bool inValid = false)
    {
        if (this.CurrentEncounter is not { } ce)
        {
            return;
        }

        if (!inValid && ce.IsValid())
        {
            ce.End = ce.LastEvent;
            foreach (var combatant in ce.Combatants)
            {
                combatant.StatusList.Clear();
            }
        }

        this.CurrentEncounter = new Encounter();
    }

    private void EndCurrentTerritoryEncounter()
    {
        if (this.CurrentTerritoryEncounter is not { } territoryEncounter)
        {
            return;
        }

        this.UpdateCurrentTerritoryEncounter();
        territoryEncounter.EndEncounter();
        this.CurrentTerritoryEncounter = null;
    }

    private void UpdateCurrentTerritoryEncounter()
    {
        if (this.CurrentTerritoryEncounter is { } currentTerritoryEncounter)
        {
            currentTerritoryEncounter.Filter = this.configuration.PartyFilter;
            currentTerritoryEncounter.CalcStats();
        }
    }

    private void AddEncounterToCurrentTerritoryEncounter(Encounter ce)
    {
        if (ce.TerritoryEncounter is not null)
        {
            return;
        }

        this.logger.Verbose("Save Encounter To CurrentTerritoryEncounter");

        TerritoryEncounter? te;
        if (this.CurrentTerritoryEncounter is null)
        {
            te = new TerritoryEncounter(ce.TerritoryName, ce.Territory, ce.Start);
            this.CurrentTerritoryEncounter = te;
            this.encounterHistory.Add(te);
        }
        else
        {
            te = this.CurrentTerritoryEncounter;
        }

        te.AddEncounter(ce);
    }

    [MemberNotNull(nameof(CurrentEncounter))]
    private void StartEncounter(DateTime? time = null)
    {
        if (this.CurrentEncounter is { } oldEncounter && this.CurrentEncounter.Start == DateTime.MinValue)
        {
            this.logger.Verbose("End CurrentEncounter - StartEncounter - start not set");
            this.EndEncounter();
        }

        var territory = this.services.ClientState.TerritoryType;
        var territoryName = this.services.DataManager.GetExcelSheet<TerritoryType>()?.GetRow(territory)?.PlaceName.Value?.Name.ToDalamudString();
        var start = time ?? DateTime.Now;
        this.CurrentEncounter = new Encounter(territoryName?.ToString() ?? string.Empty, territory, start);
    }
}