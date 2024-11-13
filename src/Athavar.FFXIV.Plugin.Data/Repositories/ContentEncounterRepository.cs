// <copyright file="ContentEncounterRepository.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Repositories;

using System.Data;
using System.Text;
using Athavar.FFXIV.Plugin.Data.Dto;
using Athavar.FFXIV.Plugin.Models.Data;
using Dalamud.Plugin.Services;
using Dapper;
using Dapper.Contrib.Extensions;
using Lumina.Excel.Sheets;

public sealed class ContentEncounterRepository : BaseRepository
{
    private readonly IDataManager dataManager;

    internal ContentEncounterRepository(IDbConnection connection, IDataManager dataManager)
        : base(connection)
        => this.dataManager = dataManager;

    public IList<ContentEncounter> GetAllContentEncounter()
    {
        var x = this.Connection.GetAll<ContentEncounterDto>();
        return this.FillData(x.Select(e => (ContentEncounter)e).ToList());
    }

    public IList<ContentEncounter> GetAllContentEncounterForPlayerId(ulong playerId) => this.GetContentEncounter(new ContentEncounterQueryParameter { PlayerId = playerId });

    public IList<ContentEncounter> GetContentEncounter(ContentEncounterQueryParameter parameter)
    {
        const string queryStart = $"SELECT * FROM {ContentEncounterDto.TableName}";
        StringBuilder sb = new();
        sb.Append(queryStart);

        var whereQuery = new List<string>();
        if (parameter.PlayerId is { } playerId)
        {
            whereQuery.Add($"{ContentEncounterDto.ColumnPlayerContentId} == {playerId}");
        }

        if (parameter.StartDate is { } startDate)
        {
            whereQuery.Add($"{ContentEncounterDto.ColumnStartDate} >= {startDate.ToUnixTimeMilliseconds()}");
        }

        if (parameter.StartDateEnd is { } startDateEnd)
        {
            whereQuery.Add($"{ContentEncounterDto.ColumnStartDate} <= {startDateEnd.ToUnixTimeMilliseconds()}");
        }

        if (parameter.TerritoryTypeId is { } territoryTypeId)
        {
            whereQuery.Add($"{ContentEncounterDto.ColumnTerritoryTypeId} = {territoryTypeId}");
        }

        if (parameter.ContentRouletteId is { } contentRouletteId)
        {
            whereQuery.Add($"{ContentEncounterDto.ColumnTerritoryTypeId} = {contentRouletteId}");
        }

        if (whereQuery.Count != 0)
        {
            sb.Append(" WHERE ");
            sb.Append(string.Join(" AND ", whereQuery));
        }

        var x = this.Connection.Query<ContentEncounterDto>(sb.ToString());
        return this.FillData(x.Select(e => (ContentEncounter)e).ToList());
    }

    public void AddContentEncounter(ContentEncounter e)
    {
        ContentEncounterDto contentEncounterDto = e;
        SetCreateTimestamp(contentEncounterDto);
        this.Connection.Insert(contentEncounterDto);
    }

    private List<ContentEncounter> FillData(List<ContentEncounter> encounters)
    {
        var rouletteSheet = this.dataManager.GetExcelSheet<ContentRoulette>()!;

        foreach (var contentRouletteGroup in encounters.Where(e => e.ContentRouletteId > 0).GroupBy(e => e.ContentRouletteId))
        {
            var rouletteId = contentRouletteGroup.Key;
            var roulette = rouletteSheet.GetRow(rouletteId);
            foreach (var contentEncounter in contentRouletteGroup)
            {
                contentEncounter.ContentRoulette = roulette;
            }
        }

        var territoryTypeSheet = this.dataManager.GetExcelSheet<TerritoryType>()!;
        foreach (var territoryTypeGroup in encounters.GroupBy(c => c.TerritoryTypeId))
        {
            var territoryTypeId = territoryTypeGroup.Key;
            var territoryType = territoryTypeSheet.GetRow(territoryTypeId)!;
            var placeName = territoryType.PlaceName.Value;
            foreach (var contentEncounter in territoryTypeGroup)
            {
                contentEncounter.TerritoryType = territoryType;
                contentEncounter.ContentFinderCondition = territoryType.ContentFinderCondition.Value;
            }
        }

        return encounters;
    }

    private ContentEncounter FillData(ContentEncounter encounter)
    {
        var territoryTypeSheet = this.dataManager.GetExcelSheet<TerritoryType>()!;
        encounter.TerritoryType = territoryTypeSheet.GetRow(encounter.TerritoryTypeId);
        encounter.ContentFinderCondition = encounter.TerritoryType?.ContentFinderCondition.Value;
        if (encounter.ContentRouletteId > 0)
        {
            var rouletteSheet = this.dataManager.GetExcelSheet<ContentRoulette>()!;
            encounter.ContentRoulette = rouletteSheet.GetRow(encounter.ContentRouletteId);
        }

        return encounter;
    }

    public sealed class ContentEncounterQueryParameter
    {
        public ulong? PlayerId { get; set; }

        public DateTimeOffset? StartDate { get; set; }

        public DateTimeOffset? StartDateEnd { get; set; }

        public uint? TerritoryTypeId { get; set; }

        public uint? ContentRouletteId { get; set; }
    }
}