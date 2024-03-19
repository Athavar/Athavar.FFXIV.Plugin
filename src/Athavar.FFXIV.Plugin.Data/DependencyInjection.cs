// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services
           .AddSingleton<RepositoryContext>()
           .AddFluentMigratorCore()
           .ConfigureRunner(
                rb => rb
                   .AddSQLite()
                   .WithGlobalConnectionString(sp => sp.GetRequiredService<IConnectionStringReader>().GetConnectionString(string.Empty))
                   .ScanIn(typeof(RepositoryContext).Assembly).For.Migrations());

        services.AddSingleton<IConnectionStringReader, ConnectionStringReader>();
        return services;
    }
}