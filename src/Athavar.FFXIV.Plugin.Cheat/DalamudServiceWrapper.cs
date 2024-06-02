// <copyright file="DalamudServiceWrapper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Cheat;

using System.Reflection;

/// <summary>
///     Wrapper and helper to access services from Dalamud.
/// </summary>
internal static class DalamudServiceWrapper
{
    private static bool init;

    private static Assembly? dalamudAssembly;

    private static Type? serviceGenericType;

    /// <summary>
    ///     Gets the instance of a service.
    /// </summary>
    /// <param name="serviceType">The type of the service.</param>
    /// <returns>returns the instance.</returns>
    public static object? GetServiceInstance(Type serviceType)
    {
        var serviceInstanceType = serviceGenericType?.MakeGenericType(serviceType);

        return serviceInstanceType?.InvokeMember("Get", BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.Public, null, null, null);
    }

    /// <summary>
    ///     Get the assembly of dalamud.
    /// </summary>
    /// <returns>The assembly.</returns>
    public static Assembly GetDalamudAssembly()
    {
        Init();
        return dalamudAssembly!;
    }

    private static void Init()
    {
        if (init)
        {
            return;
        }

        dalamudAssembly = Assembly.Load("Dalamud");

        serviceGenericType = dalamudAssembly.GetType("Dalamud.Service`1") ?? throw new Exception("Fail to get type of Dalamud.Service<>");

        init = true;
    }
}