// <copyright file="CraftQueueConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

public sealed class CraftQueueConfiguration : BasicModuleConfig<CraftQueueConfiguration>
{
    /// <summary>
    ///     Gets or sets the root folder.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("RootFolder")]
    public FolderNode RootFolder { get; set; } = new() { Name = "/" };

    /// <summary>
    ///     Gets or sets a value indicating whether to wait after a craft actions.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftWaitSkip")]
    public bool CraftWaitSkip { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to skip quality increasing actions when at 100% HQ chance.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("QualitySkip")]
    public bool QualitySkip { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to gear should be automatic repaired.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("AutoRepair")]
    public bool AutoRepair { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to materia is auto extracted.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("AutoMateriaExtract")]
    public bool AutoMateriaExtract { get; set; }

    public void CalculateDuplicateRotations()
    {
        foreach (var rotationNodes in this.GetAllNodes().OfType<RotationNode>().GroupBy(x => x.GetRotationString()))
        {
            if (rotationNodes.Count() > 1)
            {
                // duplicates
                var nodes = rotationNodes.ToArray();
                for (var i = 0; i < nodes.Length; i++)
                {
                    var node = nodes[i];
                    node.Duplicates.Clear();
                    for (var j = 0; j < nodes.Length; j++)
                    {
                        if (i == j)
                        {
                            continue;
                        }

                        node.Duplicates.Add(nodes[j]);
                    }
                }
            }
            else
            {
                // unique
                rotationNodes.First().Duplicates.Clear();
            }
        }
    }

    /// <summary>
    ///     Get all nodes in the tree.
    /// </summary>
    /// <returns>All the nodes.</returns>
    public IEnumerable<Node> GetAllNodes() => new Node[] { this.RootFolder }.Concat(this.GetAllNodes(this.RootFolder.Children));

    /// <summary>
    ///     Tries to find the parent of a node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="parent">Parent of the node or null.</param>
    /// <returns>A value indicating whether the parent was found.</returns>
    public bool TryFindParent(Node node, [NotNullWhen(true)] out FolderNode? parent)
    {
        foreach (var candidate in this.GetAllNodes())
        {
            if (candidate is FolderNode folder && folder.Children.Contains(node))
            {
                parent = folder;
                return true;
            }
        }

        parent = null;
        return false;
    }

    /// <summary>
    ///     Gets all the nodes in this subset of the tree.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The nodes in the tree.</returns>
    private IEnumerable<Node> GetAllNodes(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node is FolderNode folder)
            {
                var childNodes = this.GetAllNodes(folder.Children);
                foreach (var childNode in childNodes)
                {
                    yield return childNode;
                }
            }
        }
    }
}