// <copyright file="YesConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace

namespace Athavar.FFXIV.Plugin;

using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Keys;

/// <summary>
///     Yes module configuration.
/// </summary>
public sealed class YesConfiguration : BasicModuleConfig<YesConfiguration>
{
    /// <summary>
    ///     Gets a value indicating whether the module is enabled with functionality.
    /// </summary>
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool ModuleEnabled => this.FunctionEnabled && this.Enabled;

    /// <summary>
    ///     Gets or sets the root folder.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("RootFolder")]
    public TextFolderNode RootFolder { get; set; } = new() { Name = "/" };

    /// <summary>
    ///     Gets or sets the list root folder.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ListRootFolder")]
    public TextFolderNode ListRootFolder { get; set; } = new() { Name = "/" };

    /// <summary>
    ///     Gets or sets the talk root folder.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("TalkRootFolder")]
    public TextFolderNode TalkRootFolder { get; set; } = new() { Name = "/" };

    /// <summary>
    ///     Gets or sets the hotkey to always click yes.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ForcedYesKey")]
    public VirtualKey ForcedYesKey { get; set; } = VirtualKey.NO_KEY;

    /// <summary>
    ///     Gets or sets the hotkey to disable all functionality.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("DisableKey")]
    public VirtualKey DisableKey { get; set; } = VirtualKey.NO_KEY;

    /// <summary>
    ///     Gets or sets a value indicating whether the module functionality is enabled.
    /// </summary>
    // ReSharper disable once RedundantDefaultMemberInitializer
    [JsonInclude]
    [JsonPropertyName("FunctionEnabled")]
    public bool FunctionEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the desynth dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("DesynthDialogEnabled")]
    public bool DesynthDialogEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the desynth bulk dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("DesynthBulkDialogEnabled")]
    public bool DesynthBulkDialogEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the materialize dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("MaterializeDialogEnabled")]
    public bool MaterializeDialogEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the materia retrieve dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("MateriaRetrieveDialogEnabled")]
    public bool MateriaRetrieveDialogEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the item inspection result dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ItemInspectionResultEnabled")]
    public bool ItemInspectionResultEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the item inspection result limit, where the inspection loop will pause after that many items.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ItemInspectionResultRateLimiter")]
    public int ItemInspectionResultRateLimiter { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the retainer task ask dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("RetainerTaskAskEnabled")]
    public bool RetainerTaskAskEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the retainer task result dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("RetainerTaskResultEnabled")]
    public bool RetainerTaskResultEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the grand company supply reward dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("GrandCompanySupplyReward")]
    public bool GrandCompanySupplyReward { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the shop card dialog setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ShopCardDialog")]
    public bool ShopCardDialog { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the journal result complete setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("JournalResultCompleteEnabled")]
    public bool JournalResultCompleteEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the duty finder accept setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ContentsFinderConfirmEnabled")]
    public bool ContentsFinderConfirmEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether the duty finder one-time accept setting is enabled.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("ContentsFinderOneTimeConfirmEnabled")]
    public bool ContentsFinderOneTimeConfirmEnabled { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to remember the last pane in the inclusion shop.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("InclusionShopRememberEnabled")]
    public bool InclusionShopRememberEnabled { get; set; }

    /// <summary>
    ///     Gets or sets the first AtkValue for the inclusion shop category change event.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("InclusionShopRememberCategory")]
    public uint InclusionShopRememberCategory { get; set; }

    /// <summary>
    ///     Gets or sets the second AtkValue for the inclusion shop subcategory change event.
    /// </summary>
    [JsonInclude]
    [JsonPropertyName("InclusionShopRememberSubcategory")]
    public uint InclusionShopRememberSubcategory { get; set; }

    /// <summary>
    ///     Get all nodes in the tree.
    /// </summary>
    /// <returns>All the nodes.</returns>
    public IEnumerable<Node> GetAllNodes()
        => new Node[]
            {
                this.RootFolder,
                this.ListRootFolder,
                this.TalkRootFolder,
            }
           .Concat(this.GetAllNodes(this.RootFolder.Children))
           .Concat(this.GetAllNodes(this.ListRootFolder.Children))
           .Concat(this.GetAllNodes(this.TalkRootFolder.Children));

    /// <summary>
    ///     Gets all the nodes in this subset of the tree.
    /// </summary>
    /// <param name="nodes">Nodes to search.</param>
    /// <returns>The nodes in the tree.</returns>
    public IEnumerable<Node> GetAllNodes(IEnumerable<Node> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            if (node is TextFolderNode folder)
            {
                var children = this.GetAllNodes(folder.Children);
                foreach (var childNode in children)
                {
                    yield return childNode;
                }
            }
        }
    }

    /// <summary>
    ///     Tries to find the parent of a node.
    /// </summary>
    /// <param name="node">Node to check.</param>
    /// <param name="parent">Parent of the node or null.</param>
    /// <returns>A value indicating whether the parent was found.</returns>
    public bool TryFindParent(Node node, out TextFolderNode? parent)
    {
        foreach (var candidate in this.GetAllNodes())
        {
            if (candidate is TextFolderNode folder && folder.Children.Contains(node))
            {
                parent = folder;
                return true;
            }
        }

        parent = null;
        return false;
    }
}