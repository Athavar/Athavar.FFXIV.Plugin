// <copyright file="GateComplete.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Exceptions;

/// <summary>
///     Error thrown when a /craft or /gate command has reached the limit.
/// </summary>
internal sealed class GateComplete : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GateComplete" /> class.
    /// </summary>
    public GateComplete()
        : base("Gate reached")
    {
    }
}