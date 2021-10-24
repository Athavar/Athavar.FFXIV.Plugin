// <copyright file="TextFolderNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin
{
    using System.Collections.Generic;

    using Newtonsoft.Json;

    /// <summary>
    /// Folder node type.
    /// </summary>
    public class TextFolderNode : INode
    {
        /// <inheritdoc/>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets the child nodes of this folder.
        /// </summary>
        [JsonProperty(ItemConverterType = typeof(ConcreteNodeConverter))]
        public List<INode> Children { get; } = new();
    }
}
