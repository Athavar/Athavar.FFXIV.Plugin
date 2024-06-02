// <copyright file="LocalPlugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Models.DalamudLike;

using System.Reflection;
using Dalamud.Plugin;

public abstract class LocalPlugin
{
    /// <summary>
    ///     Gets the plugin instance.
    /// </summary>
    public abstract IDalamudPlugin? Instance { get; }

    /// <summary>
    ///     Gets the name of the plugin.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    ///     Gets the assembly name of the plugin.
    /// </summary>
    public abstract AssemblyName? AssemblyName { get; }

    /// <summary>
    ///     Gets the state of the plugin.
    /// </summary>
    public abstract PluginState State { get; }
}