// <copyright file="MacroNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

/// <summary>
///     Macro node type.
/// </summary>
public class MacroNode : INode
{
    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the contents of the macro.
    /// </summary>
    public string Contents { get; set; } = string.Empty;
}