// <copyright file="MacroSyntaxError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Exceptions;

/// <summary>
///     Error thrown when the syntax of a macro does not parse correctly.
/// </summary>
internal sealed class MacroSyntaxError : InvalidOperationException
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroSyntaxError" /> class.
    /// </summary>
    /// <param name="command">The command that failed parsing.</param>
    public MacroSyntaxError(string command)
        : base($"Syntax error: {command}")
    {
    }
}