// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Data;

using FluentMigrator.Runner;
using FluentMigrator.Runner.Initialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services)
    {
        services
           .AddSingleton<RepositoryContext>()
           .AddFluentMigratorCore()
           .ConfigureRunner(rb => rb
               .AddSQLite()
               .ScanIn(typeof(RepositoryContext).Assembly).For.Migrations());

        return services.RemoveAll(typeof(IConnectionStringReader)).AddSingleton<IConnectionStringReader, ConnectionStringReader>();
    }
}