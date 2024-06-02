// <copyright file="RepositoryContext.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data;

using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using Athavar.FFXIV.Plugin.Data.Extensions;
using Athavar.FFXIV.Plugin.Data.Repositories;
using Athavar.FFXIV.Plugin.Data.TypeHandler;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Plugin.Services;
using Dapper;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;

public sealed class RepositoryContext : IDisposable
{
    private readonly IPluginLogger logger;
    private readonly IConnectionStringReader connectionStringReader;
    private readonly IMigrationRunner migrationRunner;
    private readonly IDataManager dataManager;
    private IDbConnection? connection;

    public RepositoryContext(IPluginLogger logger, IDataManager dataManager, IConnectionStringReader connectionStringReader, IMigrationRunner migrationRunner)
    {
        this.logger = logger;
        this.dataManager = dataManager;
        this.connectionStringReader = connectionStringReader;
        this.migrationRunner = migrationRunner;

        SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());
        this.Initialize();
    }

    static RepositoryContext() => SqlMapper.AddTypeHandler(new DateTimeOffsetTypeHandler());

    public ContentEncounterRepository ContentEncounter { get; private set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.connection?.Close();
        this.connection?.Dispose();
    }

    [MemberNotNull(nameof(ContentEncounter))]
    private void Initialize()
    {
        this.logger.Debug("[RepositoryContext] Initialize");
        var connectionString = this.connectionStringReader.GetConnectionString(string.Empty);
        this.logger.Debug("[RepositoryContext] MigrateUp");
        this.migrationRunner.MigrateUp();
        this.logger.Debug("[RepositoryContext] BuildSQLiteConnection");
        this.connection = this.BuildSQLiteConnection(connectionString);
        this.logger.Debug("[RepositoryContext] Create Repositories");
        this.ContentEncounter = new ContentEncounterRepository(this.connection, this.dataManager);
    }

    // ReSharper disable once InconsistentNaming
    private IDbConnection BuildSQLiteConnection(string connectionString)
    {
        var connection = new SQLiteConnection(connectionString);
        connection.Open();

        using var cmd = connection.CreateCommand();
        cmd.ExecutePragmaCommand("journal_mode = WAL");
        cmd.ExecutePragmaCommand("foreign_keys = ON");
        cmd.ExecutePragmaCommand("synchronous = NORMAL");
        cmd.ExecutePragmaCommand("cache_size = 65536");

        return connection;
    }
}