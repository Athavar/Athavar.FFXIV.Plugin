// <copyright file="MacroPause.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Exceptions;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;

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