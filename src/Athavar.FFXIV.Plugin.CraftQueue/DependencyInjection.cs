// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCraftQueueModule(this IServiceCollection services)
    {
        services.AddSingleton<CraftQueueModule>();
        services.AddSingleton<ICraftQueueTab, CraftQueueTab>();
        services.AddSingleton<CraftQueueData>();
        services.AddSingleton<CraftQueue>();

        return services;
    }
}