// <copyright file="EventFrameworkTimeoutError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Exceptions;

using System;

/// <summary>
///     Error thrown when a timeout occurs.
/// </summary>
internal class EventFrameworkTimeoutError : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EventFrameworkTimeoutError" /> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public EventFrameworkTimeoutError(string message)
        : base(message)
    {
    }
}