// <copyright file="MacroNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;

/// <summary>
///     Macro node type.
/// </summary>
public sealed class MacroNode : Node
{
    /// <inheritdoc/>
    public override string Name { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the contents of the macro.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Contents")]
    public string Contents { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets a value indicating whether this macro should loop automatically.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftingLoop")]
    public bool CraftingLoop { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating how many loops this macro should run if looping is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftLoopCount")]
    public int CraftLoopCount { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the macro is a Lua script.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("IsLua")]
    public bool IsLua { get; set; }
}