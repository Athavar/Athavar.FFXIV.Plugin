// <copyright file="DependencyInjection.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.OpcodeWizard;

using Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddOpcodeWizard(this IServiceCollection services)
    {
        services.AddSingleton<OpcodeWizardModule>();
        services.AddSingleton<IOpcodeWizardTab, OpcodeWizardTab>();
        services.AddSingleton<DetectionProgram>();
        services.AddSingleton<ScannerRegistry>();

        services.AddSingleton<IOpcodeManager, OpcodeManager>();
        return services;
    }
}