// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Config;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDps(this IServiceCollection services)
    {
        services.AddTransient<DpsModule>();
        services.AddSingleton<NetworkHandler>();
        services.AddSingleton<EncounterManager>();
        services.AddSingleton<Utils>();
        DpsConfiguration.AddToDependencyInjection(services);
        return services;
    }
}