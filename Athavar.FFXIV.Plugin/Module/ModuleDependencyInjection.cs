namespace Athavar.FFXIV.Plugin.Module;

using Athavar.FFXIV.Plugin.Module.HuntLink;
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
}