// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddAutoSpearModule(this IServiceCollection services)
    {
        services.AddSingleton<AutoSpearModule>();
        services.AddSingleton<IAutoSpearTab, AutoSpear>();

        return services;
    }
}