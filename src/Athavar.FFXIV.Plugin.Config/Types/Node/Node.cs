// <copyright file="INode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;

/// <summary>
///     Base node interface type.
/// </summary>
[JsonDerivedType(typeof(FolderNode), 0)]
[JsonDerivedType(typeof(ListEntryNode), 1)]
[JsonDerivedType(typeof(MacroNode), 2)]
[JsonDerivedType(typeof(RotationNode), 3)]
[JsonDerivedType(typeof(TalkEntryNode), 4)]
[JsonDerivedType(typeof(TextEntryNode), 5)]
[JsonDerivedType(typeof(TextFolderNode), 6)]
public abstract class Node
{
    /// <summary>
    ///     Gets or sets the name of the node.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Name")]
    public abstract string Name { get; set; }
}