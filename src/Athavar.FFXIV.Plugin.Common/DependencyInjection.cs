// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Common.Manager;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<IDalamudServices, DalamudServices>()
           .AddSingleton<ILocalizeManager, LocalizeManager>()
           .AddSingleton<IIconCacheManager, IconCacheManager>()
           .AddSingleton<IGearsetManager, GearsetManager>()
           .AddSingleton<IChatManager, ChatManager>()
           .AddSingleton<EquipmentScanner>()
           .AddSingleton<KeyStateExtended>()
           .AddSingleton<ICommandInterface, CommandInterface>();

        return services;
    }
}