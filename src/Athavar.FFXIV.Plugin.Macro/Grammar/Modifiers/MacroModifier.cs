// <copyright file="MacroModifier.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Base class for modifiers.
/// </summary>
internal abstract class MacroModifier
{
    private static IDalamudServices? dalamudServices;

    /// <summary>
    ///     Gets the <see cref="IDalamudServices"/>.
    /// </summary>
    protected IDalamudServices DalamudServices => dalamudServices ?? throw new NullReferenceException("DalamudServices is not set");

    /// <summary>
    ///     Gets the <see cref="IPluginLogger"/>.
    /// </summary>
    protected IPluginLogger Logger => this.DalamudServices.PluginLogger;

    /// <summary>
    ///     Setup the <see cref="IServiceProvider"/> for all commands.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    internal static void SetServiceProvider(IServiceProvider serviceProvider) => dalamudServices = serviceProvider.GetRequiredService<IDalamudServices>();
}