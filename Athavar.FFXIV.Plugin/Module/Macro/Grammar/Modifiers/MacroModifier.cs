// <copyright file="MacroModifier.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;

using System;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Base class for modifiers.
/// </summary>
internal abstract class MacroModifier
{
    private static IDalamudServices? dalamudServices;

    /// <summary>
    ///     Gets the <see cref="IDalamudServices" />.
    /// </summary>
    protected static IDalamudServices DalamudServices => dalamudServices ?? throw new NullReferenceException("DalamudServices is not set");

    /// <summary>
    ///     Setup the <see cref="IServiceProvider" /> for all commands.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider" />.</param>
    internal static void SetServiceProvider(IServiceProvider serviceProvider) => dalamudServices = serviceProvider.GetRequiredService<IDalamudServices>();
}