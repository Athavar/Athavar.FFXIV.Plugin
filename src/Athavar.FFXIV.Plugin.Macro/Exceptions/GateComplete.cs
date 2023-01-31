// <copyright file="GateComplete.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Exceptions;

/// <summary>
///     Error thrown when a /craft or /gate command has reached the limit.
/// </summary>
internal class GateComplete : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="GateComplete" /> class.
    /// </summary>
    public GateComplete()
        : base("Gate reached")
    {
    }
}