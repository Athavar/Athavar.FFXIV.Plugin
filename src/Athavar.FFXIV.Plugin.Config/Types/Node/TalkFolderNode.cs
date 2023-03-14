// <copyright file="TalkFolderNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using Newtonsoft.Json;

/// <summary>
///     Folder node type.
/// </summary>
public sealed class TalkFolderNode : INode
{
    /// <summary>
    ///     Gets the child nodes of this folder.
    /// </summary>
    [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
    public List<INode> Children { get; } = new();

    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;
}