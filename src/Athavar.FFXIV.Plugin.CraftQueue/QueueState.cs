// <copyright file="QueueState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

internal enum QueueState
{
    /// <summary>
    ///     Running.
    /// </summary>
    Running,

    /// <summary>
    ///     Paused.
    /// </summary>
    Paused,

    /// <summary>
    ///     Paused.
    /// </summary>
    PausedSoon,
}