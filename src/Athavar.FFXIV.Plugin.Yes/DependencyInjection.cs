// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
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