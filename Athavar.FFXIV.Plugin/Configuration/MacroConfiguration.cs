// <copyright file="MacroConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Macro Module configuration.
    /// </summary>
    internal class MacroConfiguration
    {
        /// <summary>
        /// Gets or sets the configuration version.
        /// </summary>
        public int Version { get; set; } = 1;

        /// <summary>
        /// Gets the root folder.
        /// </summary>
        public FolderNode RootFolder { get; private set; } = new FolderNode { Name = "/" };

        /// <summary>
        /// Get all nodes in the tree.
        /// </summary>
        /// <returns>All the nodes.</returns>
        internal IEnumerable<INode> GetAllNodes()
        {
            return new INode[] { this.RootFolder }.Concat(this.GetAllNodes(this.RootFolder.Children));
        }

        /// <summary>
        /// Gets all the nodes in this subset of the tree.
        /// </summary>
        /// <param name="nodes">Nodes to search.</param>
        /// <returns>The nodes in the tree.</returns>
        internal IEnumerable<INode> GetAllNodes(IEnumerable<INode> nodes)
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

        /// <summary>
        /// Tries to find the parent of a node.
        /// </summary>
        /// <param name="node">Node to check.</param>
        /// <param name="parent">Parent of the node or null.</param>
        /// <returns>A value indicating whether the parent was found.</returns>
        internal bool TryFindParent(INode node, out FolderNode? parent)
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
    }
}
