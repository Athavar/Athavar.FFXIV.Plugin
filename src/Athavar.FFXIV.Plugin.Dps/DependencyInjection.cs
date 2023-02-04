// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using Athavar.FFXIV.Plugin.Dps.UI;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDps(this IServiceCollection services)
    {
        services.AddSingleton<DpsModule>();
        services.AddSingleton<IDpsTab, DpsTab>();
        services.AddSingleton<NetworkHandler>();
        services.AddSingleton<EncounterManager>();
        services.AddSingleton<MeterManager>();
        services.AddSingleton<Utils>();
        return services;
    }
}