// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Common.Manager;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddCommon(this IServiceCollection services)
    {
        services.AddSingleton<IDalamudServices, DalamudServices>()
           .AddSingleton<ILocalizeManager, LocalizeManager>()
           .AddSingleton<IIconManager, IconManager>()
           .AddSingleton<ICraftDataManager, CraftDataManager>()
           .AddSingleton<IGearsetManager, GearsetManager>()
           .AddSingleton<IChatManager, ChatManager>()
           .AddSingleton<EquipmentScanner>()
           .AddSingleton<IDefinitionManager, DefinitionManager>()
           .AddSingleton<IFontsManager, FontsManager>()
           .AddSingleton<IIpcManager, IpcManager>()
           .AddSingleton<IFrameworkManager, FrameworkManager>()
           .AddSingleton<IOpcodeManager, OpcodeManager>()
           .AddSingleton<IDutyManager, DutyManager>()
           .AddSingleton<ICommandInterface, CommandInterface>()
           .AddSingleton(o =>
            {
                var ds = o.GetRequiredService<IDalamudServices>();
                var resolver = new AddressResolver();
                ds.PluginLogger.Debug("Start Setup AddressResolver");
                resolver.Setup(ds.SigScanner);
                ds.PluginLogger.Debug("Finish Setup AddressResolver");
                return resolver;
            })
           .AddSingleton<IPluginLogger>(o => o.GetRequiredService<IDalamudServices>().PluginLogger)
           .AddSingleton<IDataManager>(o => o.GetRequiredService<IDalamudServices>().DataManager)
           .AddSingleton<IClientState>(o => o.GetRequiredService<IDalamudServices>().ClientState);
        CommonConfiguration.AddToDependencyInjection(services);

        return services;
    }
}