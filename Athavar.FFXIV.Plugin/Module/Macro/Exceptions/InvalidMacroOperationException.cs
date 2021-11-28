// <copyright file="InvalidMacroOperationException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Exceptions;

using System;

/// <summary>
///     Error thrown when an invalid macro operation occurs.
/// </summary>
internal class InvalidMacroOperationException : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidMacroOperationException" /> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public InvalidMacroOperationException(string message)
        : base(message)
    {
    }
}