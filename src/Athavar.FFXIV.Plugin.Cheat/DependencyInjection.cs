// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCheatModule(this IServiceCollection services)
    {
        services.AddSingleton<CheatModule>();
        services.AddSingleton<PluginManagerWrapper>();

        return services;
    }
}