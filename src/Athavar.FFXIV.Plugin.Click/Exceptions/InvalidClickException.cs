// <copyright file="InvalidClickException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Exceptions;

/// <summary>
///     Base exception for click errors.
/// </summary>
public class InvalidClickException : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidClickException" /> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    public InvalidClickException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="InvalidClickException" /> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    /// <param name="innerException">Causing exception.</param>
    public InvalidClickException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}