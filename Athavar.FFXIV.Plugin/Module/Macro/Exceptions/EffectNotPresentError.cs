// <copyright file="EffectNotPresentError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Exceptions;

using System;

/// <summary>
///     Error thrown when an effect is not present.
/// </summary>
internal class EffectNotPresentError : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="EffectNotPresentError" /> class.
    /// </summary>
    /// <param name="message">Message to show.</param>
    public EffectNotPresentError(string message)
        : base(message)
    {
    }
}