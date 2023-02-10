// <copyright file="MacroNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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

    /// <summary>
    ///     Gets or sets a value indicating whether this macro should loop automatically.
    /// </summary>
    public bool CraftingLoop { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating how many loops this macro should run if looping is enabled.
    /// </summary>
    public int CraftLoopCount { get; set; } = 0;

    /// <summary>
    ///     Gets or sets a value indicating whether the macro is a Lua script.
    /// </summary>
    public bool IsLua { get; set; } = false;
}