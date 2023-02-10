// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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