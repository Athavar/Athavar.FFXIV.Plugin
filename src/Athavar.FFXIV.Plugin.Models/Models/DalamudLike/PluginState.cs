// <copyright file="PluginState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.DalamudLike;

/// <summary>
///     Values representing plugin load state.
/// </summary>
public enum PluginState
{
    /// <summary>
    ///     Plugin is defined, but unloaded.
    /// </summary>
    Unloaded,

    /// <summary>
    ///     Plugin has thrown an error during unload.
    /// </summary>
    UnloadError,

    /// <summary>
    ///     Currently unloading.
    /// </summary>
    Unloading,

    /// <summary>
    ///     Load is successful.
    /// </summary>
    Loaded,

    /// <summary>
    ///     Plugin has thrown an error during loading.
    /// </summary>
    LoadError,

    /// <summary>
    ///     Currently loading.
    /// </summary>
    Loading,

    /// <summary>
    ///     This plugin couldn't load one of its dependencies.
    /// </summary>
    DependencyResolutionFailed,
}