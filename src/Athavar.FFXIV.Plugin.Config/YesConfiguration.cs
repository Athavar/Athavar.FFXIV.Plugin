// <copyright file="YesConfiguration.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

using Dalamud.Game.ClientState.Keys;
using Newtonsoft.Json;

/// <summary>
///     Yes module configuration.
/// </summary>
public class YesConfiguration : BasicModuleConfig
{
    /// <summary>
    ///     Gets the root folder.
    /// </summary>
    public TextFolderNode RootFolder { get; } = new() { Name = "/" };

    /// <summary>
    ///     Gets the list root folder.
    /// </summary>
    public TextFolderNode ListRootFolder { get; } = new() { Name = "/" };

    /// <summary>
    ///     Gets the talk root folder.
    /// </summary>
    public TextFolderNode TalkRootFolder { get; } = new() { Name = "/" };

    /// <summary>
    ///     Gets a value indicating whether the module is enabled with functionality.
    /// </summary>
    [JsonIgnore]
    public new bool Enabled => this.FunctionEnabled && this.ModuleEnabled;

    /// <summary>
    ///     Gets or sets the hotkey to always click yes.
    /// </summary>
    public VirtualKey ForcedYesKey { get; set; } = VirtualKey.NO_KEY;

    /// <summary>
    ///     Gets or sets the hotkey to disable all functionality.
    /// </summary>
    public VirtualKey DisableKey { get; set; } = VirtualKey.NO_KEY;

    /// <summary>
    ///     Gets or sets the configuration version.
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    ///     Gets or sets a value indicating whether the module is enabled.
    /// </summary>
    public bool ModuleEnabled { get; set; } = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the module functionality is enabled.
    /// </summary>
    public bool FunctionEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the desynth dialog setting is enabled.
    /// </summary>
    public bool DesynthDialogEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the desynth bulk dialog setting is enabled.
    /// </summary>
    public bool DesynthBulkDialogEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the materialize dialog setting is enabled.
    /// </summary>
    public bool MaterializeDialogEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the materia retrieve dialog setting is enabled.
    /// </summary>
    public bool MateriaRetrieveDialogEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the item inspection result dialog setting is enabled.
    /// </summary>
    public bool ItemInspectionResultEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets the item inspection result limit, where the inspection loop will pause after that many items.
    /// </summary>
    public int ItemInspectionResultRateLimiter { get; set; } = 0;

    /// <summary>
    ///     Gets or sets a value indicating whether the retainer task ask dialog setting is enabled.
    /// </summary>
    public bool RetainerTaskAskEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the retainer task result dialog setting is enabled.
    /// </summary>
    public bool RetainerTaskResultEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the grand company supply reward dialog setting is enabled.
    /// </summary>
    public bool GrandCompanySupplyReward { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the shop card dialog setting is enabled.
    /// </summary>
    public bool ShopCardDialog { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the journal result complete setting is enabled.
    /// </summary>
    public bool JournalResultCompleteEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the duty finder accept setting is enabled.
    /// </summary>
    public bool ContentsFinderConfirmEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether the duty finder one-time accept setting is enabled.
    /// </summary>
    public bool ContentsFinderOneTimeConfirmEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether to remember the last pane in the inclusion shop.
    /// </summary>
    public bool InclusionShopRememberEnabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets the first AtkValue for the inclusion shop category change event.
    /// </summary>
    public uint InclusionShopRememberCategory { get; set; } = 0;

    /// <summary>
    ///     Gets or sets the second AtkValue for the inclusion shop subcategory change event.
    /// </summary>
    public uint InclusionShopRememberSubcategory { get; set; } = 0;

    /// <summary>
    ///     Get all nodes in the tree.
    /// </summary>
    /// <returns>All the nodes.</returns>
    public IEnumerable<INode> GetAllNodes()
        => new INode[]
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
    public IEnumerable<INode> GetAllNodes(IEnumerable<INode> nodes)
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
    public bool TryFindParent(INode node, out TextFolderNode? parent)
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