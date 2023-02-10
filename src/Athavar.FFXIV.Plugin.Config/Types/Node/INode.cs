// <copyright file="INode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
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