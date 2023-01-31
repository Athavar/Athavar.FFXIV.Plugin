// <copyright file="LoopState.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Managers;

/// <summary>
///     The state of the macro manager.
/// </summary>
internal enum LoopState
{
    /// <summary>
    ///     Not logged in.
    /// </summary>
    NotLoggedIn,

    /// <summary>
    ///     Waiting.
    /// </summary>
    Waiting,

    /// <summary>
    ///     Running.
    /// </summary>
    Running,

    /// <summary>
    ///     Paused.
    /// </summary>
    Paused,

    /// <summary>
    ///     Stopped.
    /// </summary>
    Stopped,
}