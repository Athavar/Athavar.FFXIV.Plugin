// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Config;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddModuleConfiguration(this IServiceCollection services)
    {
        CommonConfiguration.AddToDependencyInjection(services);
        AutoSpearConfiguration.AddToDependencyInjection(services);
        CraftQueueConfiguration.AddToDependencyInjection(services);
        DpsConfiguration.AddToDependencyInjection(services);
        DutyHistoryConfiguration.AddToDependencyInjection(services);
        ImporterConfiguration.AddToDependencyInjection(services);
        InstancinatorConfiguration.AddToDependencyInjection(services);
        MacroConfiguration.AddToDependencyInjection(services);
        OpcodeWizardConfiguration.AddToDependencyInjection(services);
        SliceIsRightConfiguration.AddToDependencyInjection(services);
        YesConfiguration.AddToDependencyInjection(services);

        return services;
    }
}