// <copyright file="FolderNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;
using Newtonsoft.Json;

/// <summary>
///     Folder node type.
/// </summary>
public sealed class FolderNode : Node
{
    /// <summary>
    ///     Gets the children inside this folder.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("Children")]
    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<Node> Children { get; init; } = new();

    /// <inheritdoc/>
    public override string Name { get; set; } = string.Empty;
}