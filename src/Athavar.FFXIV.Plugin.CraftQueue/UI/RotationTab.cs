// <copyright file="RotationTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using System.Numerics;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using ImGuiNET;

internal sealed class RotationTab : Tab
{
    private readonly Regex incrementalName = new(@"(?<all> \((?<index>\d+)\))$", RegexOptions.Compiled);
    private readonly IPluginLogger logger;
    private readonly IChatManager chatManager;
    private readonly IIconManager iconManager;
    private readonly ICraftDataManager craftDataManager;
    private Node? draggedNode;
    private RotationNode? activeRotationNode;
    private CraftingMacro? activeRotationMacro;

    private bool editChanged;
    private string activeRotationContent = string.Empty;

    public RotationTab(IPluginLogger logger, CraftQueueConfiguration configuration, IChatManager chatManager, IIconManager iconManager, ICraftDataManager craftDataManager, ClientLanguage clientLanguage)
    {
        this.logger = logger;
        this.Configuration = configuration;
        this.chatManager = chatManager;
        this.iconManager = iconManager;
        this.craftDataManager = craftDataManager;
        this.ClientLanguage = clientLanguage;

        this.Configuration.CalculateDuplicateRotations();
    }

    /// <inheritdoc/>
    public override string Name => "Rotation";

    /// <inheritdoc/>
    public override string Identifier => "Tab-RotationsOptions";

    private CraftQueueConfiguration Configuration { get; }

    private FolderNode RootFolder => this.Configuration.RootFolder;

    private ClientLanguage ClientLanguage { get; }

    /// <inheritdoc/>
    public override void Draw()
    {
        ImGui.Columns(2);

        if (ImGui.BeginChild("##craftQueue-rotation-tree", ImGui.GetContentRegionAvail(), false))
        {
            this.DisplayRotationTree();
            ImGui.EndChild();
        }

        ImGui.NextColumn();

        this.DisplayRotationInfo();
        this.DisplayRotationEdit();

        ImGui.Columns(1);
    }

    private void DisplayRotationTree() => this.DisplayNode(this.RootFolder);

    private void DisplayRotationInfo()
    {
        var node = this.activeRotationNode;
        if (node is null || this.activeRotationMacro is null)
        {
            return;
        }

        ImGui.Text("Current Rotation");

        ImGui.PushItemWidth(-1);

        var style = ImGui.GetStyle();
        var runningHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 3) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 2);
        if (ImGui.BeginChild("Rotation##display-rotation", new Vector2(-1, runningHeight)))
        {
            var rotations = this.activeRotationMacro.Rotation;
            for (var index = 0; index < rotations.Length; index++)
            {
                var x = ImGui.GetContentRegionAvail().X;

                var action = rotations[index];
                var craftSkillData = this.craftDataManager.GetCraftSkillData(action);
                var tex = this.iconManager.GetIcon(craftSkillData.IconIds[0], ITextureProvider.IconFlags.None);
                ImGui.Image(tex!.ImGuiHandle, new Vector2(tex.Height, tex.Width));
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(craftSkillData.Name[this.ClientLanguage]);
                    ImGui.EndTooltip();
                }

                if (index != rotations.Length - 1 && 80.0 + ImGui.GetStyle().ItemSpacing.X <= x)
                {
                    ImGui.SameLine();
                }
            }

            ImGui.EndChild();
        }

        ImGui.PopItemWidth();
    }

    private void DisplayRotationEdit()
    {
        var node = this.activeRotationNode;
        if (node is null)
        {
            return;
        }

        ImGui.Text("Rotation Editor");

        var edit = this.editChanged;
        if (edit)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, ImGuiColors.DalamudRed);
        }

        if (ImGuiEx.IconButton(FontAwesomeIcon.Save, "Save"))
        {
            if (this.activeRotationNode is not null)
            {
                node.Save(this.activeRotationMacro);
                this.editChanged = false;
                this.Configuration.Save();
                this.Configuration.CalculateDuplicateRotations();
            }
        }

        if (edit)
        {
            ImGui.PopStyleColor();
        }

        ImGui.SameLine();

        if (ImGuiEx.IconButton(FontAwesomeIcon.FileImport, "Import from clipboard (hold control to append)"))
        {
            var io = ImGui.GetIO();
            var ctrlHeld = io.KeyCtrl;

            string text;
            try
            {
                text = ImGui.GetClipboardText();
            }
            catch (NullReferenceException ex)
            {
                text = string.Empty;
                this.chatManager.PrintErrorMessage("[Macro] Could not import from clipboard.");
                this.logger.Error(ex, "Clipboard import error");
            }

            // Replace \r with \r\n, usually from copy/pasting from the in-game macro window
            var rex = new Regex("\r(?!\n)", RegexOptions.Compiled);
            var matches = from Match match in rex.Matches(text)
                          let index = match.Index
                          orderby index descending
                          select index;
            foreach (var index in matches)
            {
                text = text.Remove(index, 1).Insert(index, "\r\n");
            }

            var craftMacro = this.craftDataManager.ParseCraftingMacro(text);
            if (ctrlHeld && this.activeRotationMacro is not null)
            {
                this.activeRotationMacro = new CraftingMacro(this.activeRotationMacro.Rotation.Concat(craftMacro.Rotation).ToArray());
            }
            else
            {
                this.activeRotationMacro = craftMacro;
            }

            this.activeRotationContent = this.craftDataManager.CreateTextMacro(this.activeRotationMacro, this.ClientLanguage);

            this.editChanged = true;
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.TimesCircle, "Close"))
        {
            this.activeRotationNode = null;
            this.activeRotationContent = string.Empty;
            this.editChanged = false;
        }

        ImGui.PushItemWidth(-1);
        if (ImGui.InputTextMultiline($"##{node.Name}-editor", ref this.activeRotationContent, 100_000, new Vector2(-1, -1)))
        {
            this.editChanged = true;
            this.activeRotationMacro = this.craftDataManager.ParseCraftingMacro(this.activeRotationContent);
        }

        ImGui.PopItemWidth();
    }

    private void DisplayNode(Node node)
    {
        ImGui.PushID(node.Name);

        if (node is FolderNode folderNode)
        {
            this.DisplayFolderNode(folderNode);
        }
        else if (node is RotationNode rotationNode)
        {
            this.DisplayRotationNode(rotationNode);
        }

        ImGui.PopID();
    }

    private void DisplayFolderNode(FolderNode node)
    {
        if (node == this.RootFolder)
        {
            ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
        }

        var expanded = ImGui.TreeNodeEx($"{node.Name}##tree");

        this.DisplayNodePopup(node);
        this.NodeDragDrop(node);

        if (expanded)
        {
            foreach (var childNode in node.Children.ToArray())
            {
                this.DisplayNode(childNode);
            }

            ImGui.TreePop();
        }
    }

    private void DisplayRotationNode(RotationNode node)
    {
        var flags = ImGuiTreeNodeFlags.Leaf;
        if (node == this.activeRotationNode)
        {
            flags |= ImGuiTreeNodeFlags.Selected;
        }

        if (node.Duplicates.Any())
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
        }

        ImGui.TreeNodeEx($"{node.Name}##tree", flags);

        if (node.Duplicates.Any())
        {
            ImGui.PopStyleColor();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("Duplicate rotations:");
                foreach (var duplicate in node.Duplicates)
                {
                    ImGui.BulletText(duplicate.Name);
                }

                ImGui.EndTooltip();
            }
        }

        this.DisplayNodePopup(node);
        this.NodeDragDrop(node);

        if (ImGui.IsItemClicked())
        {
            // select rotation
            this.activeRotationNode = node;
            this.activeRotationMacro = CraftingSkill.ParseMacro(node.Rotations);
            this.activeRotationContent = this.craftDataManager.CreateTextMacro(this.activeRotationMacro, this.ClientLanguage);
            this.editChanged = false;
        }

        ImGui.TreePop();
    }

    private string GetUniqueNodeName(string name)
    {
        var nodeNames = this.Configuration.GetAllNodes()
           .Select(node => node.Name)
           .ToList();

        while (nodeNames.Contains(name))
        {
            var match = this.incrementalName.Match(name);
            if (match.Success)
            {
                var all = match.Groups["all"].Value;
                var index = int.Parse(match.Groups["index"].Value) + 1;
                name = name[..^all.Length];
                name = $"{name} ({index})";
            }
            else
            {
                name = $"{name} (1)";
            }
        }

        return name.Trim();
    }

    private void NodeDragDrop(Node node)
    {
        if (node != this.RootFolder)
        {
            if (ImGui.BeginDragDropSource())
            {
                this.draggedNode = node;
                ImGui.Text(node.Name);
                ImGui.SetDragDropPayload("NodePayload", nint.Zero, 0);
                ImGui.EndDragDropSource();
            }
        }

        if (ImGui.BeginDragDropTarget())
        {
            var payload = ImGui.AcceptDragDropPayload("NodePayload");

            bool nullPtr;
            unsafe
            {
                nullPtr = payload.NativePtr == null;
            }

            var targetNode = node;
            if (!nullPtr && payload.IsDelivery() && this.draggedNode != null)
            {
                if (!this.Configuration.TryFindParent(this.draggedNode, out var draggedNodeParent))
                {
                    throw new Exception($"Could not find parent of node \"{this.draggedNode.Name}\"");
                }

                if (targetNode is FolderNode targetFolderNode && !ImGui.IsKeyDown(ImGuiKey.ModShift))
                {
                    draggedNodeParent!.Children.Remove(this.draggedNode);
                    targetFolderNode.Children.Add(this.draggedNode);
                    this.Configuration.Save();
                }
                else
                {
                    if (!this.Configuration.TryFindParent(targetNode, out var targetNodeParent))
                    {
                        throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");
                    }

                    var targetNodeIndex = targetNodeParent!.Children.IndexOf(targetNode);
                    if (targetNodeParent == draggedNodeParent)
                    {
                        var draggedNodeIndex = targetNodeParent.Children.IndexOf(this.draggedNode);
                        if (draggedNodeIndex < targetNodeIndex)
                        {
                            targetNodeIndex -= 1;
                        }
                    }

                    draggedNodeParent!.Children.Remove(this.draggedNode);
                    targetNodeParent.Children.Insert(targetNodeIndex, this.draggedNode);
                    this.Configuration.Save();
                }

                this.draggedNode = null;
            }

            ImGui.EndDragDropTarget();
        }
    }

    private void DisplayNodePopup(Node node)
    {
        if (ImGui.BeginPopupContextItem($"##{node.Name}-popup"))
        {
            var name = node.Name;
            if (ImGui.InputText("##rename", ref name, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
            {
                node.Name = this.GetUniqueNodeName(name);
                this.Configuration.Save();
            }

            if (node is FolderNode folderNode)
            {
                if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add Rotation"))
                {
                    var newNode = new RotationNode { Name = this.GetUniqueNodeName("Untitled rotation") };
                    folderNode.Children.Add(newNode);
                    this.Configuration.Save();

                    this.Configuration.CalculateDuplicateRotations();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                {
                    var newNode = new FolderNode { Name = this.GetUniqueNodeName("Untitled folder") };
                    folderNode.Children.Add(newNode);
                    this.Configuration.Save();
                }
            }

            // ReSharper disable once PossibleUnintendedReferenceComparison
            if (node != this.RootFolder)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Copy, "Copy Name"))
                {
                    ImGui.SetClipboardText(node.Name);
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.Configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.Configuration.Save();

                        if (node is RotationNode)
                        {
                            this.Configuration.CalculateDuplicateRotations();
                        }
                    }
                }

                ImGui.SameLine();
            }

            ImGui.EndPopup();
        }
    }
}