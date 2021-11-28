// <copyright file="ConditionNotFulfilledError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Exceptions;

using System;

/// <summary>
///     Error thrown when an condition is not present.
/// </summary>
internal class ConditionNotFulfilledError : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConditionNotFulfilledError" /> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public ConditionNotFulfilledError(string message)
        : base(message)
    {
    }
}