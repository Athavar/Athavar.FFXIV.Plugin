// <copyright file="ModuleDependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module;

using Athavar.FFXIV.Plugin.Module.HuntLink;
using Athavar.FFXIV.Plugin.Module.Instancinator;
using Athavar.FFXIV.Plugin.Module.ItemInspector;
using Athavar.FFXIV.Plugin.Module.Macro;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;
using Athavar.FFXIV.Plugin.Module.Yes;
using Microsoft.Extensions.DependencyInjection;

public static class ModuleDependencyInjection
{
    internal static IServiceCollection AddMacroModule(this IServiceCollection services)
    {
        services.AddSingleton<MacroModule>();
        services.AddSingleton<MacroConfigTab>();
        services.AddSingleton<ConditionCheck>();
        services.AddSingleton<MacroManager>();
        services.AddSingleton<MacroHelpWindow>();

        return services;
    }

    internal static IServiceCollection AddYesModule(this IServiceCollection services)
    {
        services.AddSingleton<YesModule>();
        services.AddSingleton<YesConfigTab>();
        services.AddSingleton<ZoneListWindow>();

        return services;
    }

    internal static IServiceCollection AddHuntLinkModule(this IServiceCollection services)
    {
        services.AddSingleton<HuntLinkModule>();
        services.AddSingleton<HuntLinkDatastore>();
        services.AddSingleton<HuntLinkTab>();

        return services;
    }

    internal static IServiceCollection AddItemInspectorModule(this IServiceCollection services)
    {
        services.AddSingleton<ItemInspectorModule>();
        services.AddSingleton<ItemInspectorTab>();

        return services;
    }

    internal static IServiceCollection AddInstancinatorModule(this IServiceCollection services)
    {
        services.AddSingleton<InstancinatorModule>();
        services.AddSingleton<InstancinatorWindow>();

        return services;
    }
}