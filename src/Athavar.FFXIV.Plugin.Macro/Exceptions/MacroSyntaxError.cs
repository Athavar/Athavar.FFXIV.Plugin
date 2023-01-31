// <copyright file="MacroSyntaxError.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Exceptions;

/// <summary>
///     Error thrown when the syntax of a macro does not parse correctly.
/// </summary>
internal class MacroSyntaxError : InvalidOperationException
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