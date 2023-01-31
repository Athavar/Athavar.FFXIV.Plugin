// <copyright file="QueueState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
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