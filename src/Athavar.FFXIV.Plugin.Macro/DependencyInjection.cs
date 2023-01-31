// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Macro;

using Athavar.FFXIV.Plugin.Macro.Managers;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddMacroModule(this IServiceCollection services)
    {
        services.AddSingleton<MacroModule>();
        services.AddSingleton<IMacroConfigTab, MacroConfigTab>();
        services.AddSingleton<ConditionCheck>();
        services.AddSingleton<MacroManager>();
        services.AddSingleton<MacroHelpWindow>();

        return services;
    }
}