// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddDps(this IServiceCollection services)
    {
        services.AddSingleton<NetworkHandler>();
        services.AddSingleton<EncounterManager>();
        services.AddSingleton<Utils>();
        return services;
    }
}