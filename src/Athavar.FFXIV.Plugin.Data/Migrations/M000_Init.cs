// <copyright file="M000_Init.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data.Migrations;

using Athavar.FFXIV.Plugin.Data.Dto;
using Athavar.FFXIV.Plugin.Data.Extensions;
using FluentMigrator;

[Migration(1)]
public class M000_Init : Migration
{
    public override void Up() => this.CreateContentEncounterTable();

    public override void Down()
    {
        this.Delete.Table(ContentEncounterDto.TableName);
    }

    private void CreateContentEncounterTable()
    {
        this.Create.Table(ContentEncounterDto.TableName)
           .WithId()
           .WithTimeStamp()
           .WithColumn(ContentEncounterDto.ColumnContentRouletteId).AsUInt32(ContentEncounterDto.ColumnContentRouletteId).NotNullable()
           .WithColumn(ContentEncounterDto.ColumnTerritoryTypeId).AsUInt32(ContentEncounterDto.ColumnTerritoryTypeId).NotNullable()
           .WithColumn(ContentEncounterDto.ColumnClassJobId).AsUInt32(ContentEncounterDto.ColumnClassJobId).NotNullable()
           .WithColumn(ContentEncounterDto.ColumnCompleted).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnStartDate).AsInt64().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnEndDate).AsInt64().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnUnrestrictedParty).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnMinimalIL).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnLevelSync).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnSilenceEcho).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnExplorerMode).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnJoinInProgress).AsBoolean().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnQueuePlayerCount).AsInt32().NotNullable()
           .WithColumn(ContentEncounterDto.ColumnWipes).AsInt32().NotNullable();
    }
}