// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Instancinator;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInstancinatorModule(this IServiceCollection services)
    {
        services.AddSingleton<InstancinatorModule>();
        services.AddSingleton<InstancinatorWindow>();
        services.AddSingleton<IInstancinatorTab, InstancinatorTab>();

        return services;
    }
}