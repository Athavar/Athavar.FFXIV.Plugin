// <copyright file="EncounterManager.Control.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Dps.Data;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

internal partial class EncounterManager
{
    public void EndEncounter(bool inValid = false)
    {
        if (this.CurrentEncounter is null)
        {
            return;
        }

        var ce = this.CurrentEncounter;
        this.CurrentEncounter = new Encounter();

        if (!inValid && ce.IsValid())
        {
            ce.End = ce.LastEvent;
            // File.WriteAllText($"Z:\\home\\athavar\\.xlcore\\logs\\dps\\{ce.TitleStart}.log", string.Join('\n', this.Log));
        }
    }

    private void EndCurrentTerritoryEncounter()
    {
        if (this.CurrentTerritoryEncounter is null)
        {
            return;
        }

        this.UpdateCurrentTerritoryEncounter();
        this.CurrentTerritoryEncounter.EndEncounter();
        this.CurrentTerritoryEncounter = null;
    }

    private void UpdateCurrentTerritoryEncounter()
    {
        var currentTerritoryEncounter = this.CurrentTerritoryEncounter;
        if (currentTerritoryEncounter != null)
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

        TerritoryEncounter? te;
        if (this.CurrentTerritoryEncounter is null)
        {
            te = new TerritoryEncounter(ce.TerritoryName, ce.Territory, ce.Start);
            this.CurrentTerritoryEncounter = te;
            this.encounterHistory.Add(te);
        }

        te = this.CurrentTerritoryEncounter;
        te.AddEncounter(ce);
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
        this.CurrentEncounter = new Encounter(territoryName?.ToString() ?? string.Empty, territory, start);
    }
}