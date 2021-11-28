// <copyright file="ClickNotFoundError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Lib.ClickLib;

using System;

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