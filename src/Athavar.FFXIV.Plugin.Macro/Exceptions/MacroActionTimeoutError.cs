// <copyright file="MacroActionTimeoutError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Exceptions;

/// <summary>
///     Error thrown when an action does not receive a timely response.
/// </summary>
internal class MacroActionTimeoutError : MacroCommandError
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroActionTimeoutError" /> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public MacroActionTimeoutError(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroActionTimeoutError" /> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public MacroActionTimeoutError(string message, Exception? innerException)
        : base(message, innerException)
    {
    }
}