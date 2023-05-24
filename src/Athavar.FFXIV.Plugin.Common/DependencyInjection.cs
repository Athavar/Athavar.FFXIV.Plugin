// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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
           .AddSingleton<IIconManager, IconManager>()
           .AddSingleton<ICraftDataManager, CraftDataManager>()
           .AddSingleton<IGearsetManager, GearsetManager>()
           .AddSingleton<IChatManager, ChatManager>()
           .AddSingleton<EquipmentScanner>()
           .AddSingleton<KeyStateExtended>()
           .AddSingleton<IDefinitionManager, DefinitionManager>()
           .AddSingleton<IFontsManager, FontsManager>()
           .AddSingleton<IIpcManager, IpcManager>()
           .AddSingleton<ICommandInterface, CommandInterface>();

        return services;
    }
}