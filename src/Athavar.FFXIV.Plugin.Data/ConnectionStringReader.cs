// <copyright file="ConnectionStringReader.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data;

using Dalamud.Plugin;
using FluentMigrator.Runner.Initialization;

internal sealed class ConnectionStringReader : IConnectionStringReader
{
    private readonly string connectionString;

    public ConnectionStringReader(IDalamudPluginInterface pluginInterface) => this.connectionString = this.BuildSQLiteConnectionString(pluginInterface.ConfigDirectory.FullName, "data");

    public int Priority => 999;

    public string GetConnectionString(string connectionStringOrName) => this.connectionString;

    // ReSharper disable once InconsistentNaming
    private string BuildSQLiteConnectionString(string dbDirectory, string dbName)
    {
        Directory.CreateDirectory(dbDirectory);
        return $"Data Source={Path.Combine(dbDirectory, $"{dbName}.db")};Version=3;";
    }
}