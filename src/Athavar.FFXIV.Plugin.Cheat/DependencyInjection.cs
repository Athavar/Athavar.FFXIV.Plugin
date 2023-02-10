// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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