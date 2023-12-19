// <copyright file="MacroConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

/// <summary>
///     Macro Module configuration.
/// </summary>
public sealed class MacroConfiguration : BasicModuleConfig<MacroConfiguration>
{
    /// <summary>
    ///     Gets or sets the root folder.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("RootFolder")]
    public FolderNode RootFolder { get; set; } = new() { Name = "/" };

    /// <summary>
    ///     Gets or sets a value indicating whether to skip craft actions when not crafting.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftSkip")]
    public bool CraftSkip { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether to skip craft actions when not crafting.
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
    ///     Gets or sets a value indicating whether to count the /loop number as the total iterations, rather than the amount
    ///     to loop.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("LoopTotal")]
    public bool LoopTotal { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to always echo /loop commands.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("LoopEcho")]
    public bool LoopEcho { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the mono front should be disabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("DisableMonospaced")]
    public bool DisableMonospaced { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to use the "CraftLoop" template.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("UseCraftLoopTemplate")]
    public bool UseCraftLoopTemplate { get; set; }

    /// <summary>
    ///     Gets or sets the "CraftLoop" template.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftLoopTemplate")]
    public string CraftLoopTemplate { get; set; } =
        "/craft {{count}}\n" +
        "/waitaddon \"RecipeNote\" <maxwait.5>\n" +
        "/click \"synthesize\"\n" +
        "/waitaddon \"Synthesis\" <maxwait.5>\n" +
        "{{macro}}\n" +
        "/loop";

    /// <summary>
    ///     Gets or sets a value indicating whether to start crafting loops from the recipe note window.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftLoopFromRecipeNote")]
    public bool CraftLoopFromRecipeNote { get; set; } = true;

    /// <summary>
    ///     Gets or sets the maximum wait value for the "CraftLoop" maxwait modifier.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftLoopMaxWait")]
    public int CraftLoopMaxWait { get; set; } = 5;

    /// <summary>
    ///     Gets or sets a value indicating whether the "CraftLoop" loop should have an echo modifier.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("CraftLoopEcho")]
    public bool CraftLoopEcho { get; set; }

    /// <summary>
    ///     Gets or sets the maximum number of retries when an action does not receive a timely response.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("MaxTimeoutRetries")]
    public int MaxTimeoutRetries { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether errors should be audible.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("NoisyErrors")]
    public bool NoisyErrors { get; set; }

    /// <summary>
    ///     Gets or sets the beep frequency.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("BeepFrequency")]
    public int BeepFrequency { get; set; } = 900;

    /// <summary>
    ///     Gets or sets the beep duration.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("BeepDuration")]
    public int BeepDuration { get; set; } = 250;

    /// <summary>
    ///     Gets or sets the beep count.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("BeepCount")]
    public int BeepCount { get; set; } = 3;

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
    internal IEnumerable<Node> GetAllNodes(IEnumerable<Node> nodes)
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