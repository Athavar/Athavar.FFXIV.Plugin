// <copyright file="ClickNotFoundError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Click.Exceptions;

/// <summary>
///     An exception thrown when a click cannot be found.
/// </summary>
public class ClickNotFoundError : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ClickNotFoundError" /> class.
    /// </summary>
    /// <param name="message">Error message.</param>
    public ClickNotFoundError(string message)
        : base(message)
    {
    }
}