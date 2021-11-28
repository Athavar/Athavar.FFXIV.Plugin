// <copyright file="INode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

/// <summary>
///     Base node interface type.
/// </summary>
public interface INode
{
    /// <summary>
    ///     Gets or sets the name of the node.
    /// </summary>
    public string Name { get; set; }
}