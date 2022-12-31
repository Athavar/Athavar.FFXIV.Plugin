// <copyright file="MacroPause.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Exceptions;

using System;
using Athavar.FFXIV.Plugin.Manager.Interface;

/// <summary>
///     Error thrown when a macro needs to pause, but not treat it like an error.
/// </summary>
internal class MacroPause : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroPause" /> class.
    /// </summary>
    /// <param name="command">The reason for stopping.</param>
    /// <param name="color">SeString color.</param>
    public MacroPause(string command, IChatManager.UiColor color)
        : base($"Macro paused: {command}")
        => this.Color = color;

    /// <summary>
    ///     Gets the color.
    /// </summary>
    public IChatManager.UiColor Color { get; }
}