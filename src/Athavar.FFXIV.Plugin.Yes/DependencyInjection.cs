// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Yes;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddYesModule(this IServiceCollection services)
    {
        services.AddSingleton<YesModule>();
        services.AddSingleton<IYesConfigTab, YesConfigTab>();
        services.AddSingleton<ZoneListWindow>();

        return services;
    }
}