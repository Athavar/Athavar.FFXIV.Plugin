﻿namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using Athavar.FFXIV.Plugin;
    using Athavar.FFXIV.Plugin.Utils;
    using Dalamud.Interface;
    using ImGuiNET;

    /// <summary>
    /// Yes Module configuration Tab.
    /// </summary>
    internal class YesConfigTab
    {
        private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);
        private readonly YesModule module;

        private INode? draggedNode = null;
        private string debugClickName = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="YesConfigTab"/> class.
        /// </summary>
        /// <param name="module">The <see cref="YesModule"/>.</param>
        public YesConfigTab(YesModule module)
        {
            this.module = module;
        }

        private TextFolderNode RootFolder => this.module.Configuration.RootFolder;

        public void DrawTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Yes"), ImGui.EndTabItem))
            {
                return;
            }

#if DEBUG
            this.UiBuilder_TestButton();
#endif

            var enabled = this.module.Configuration.Enabled;
            if (ImGui.Checkbox($"Enabled", ref enabled))
            {
                this.module.Configuration.Enabled = enabled;
                this.module.Configuration.Save();
            }

            this.UiBuilder_TextNodeButtons();
            this.UiBuilder_TextNodes();
            this.UiBuilder_ItemsWithoutText();
        }

        #region Testing

        private void UiBuilder_TestButton()
        {
            ImGui.InputText("ClickName", ref this.debugClickName, 100);
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Check, "Submit"))
            {
                try
                {
                    this.debugClickName ??= string.Empty;
                    ClickLib.Click.SendClick(this.debugClickName.Trim());
                    this.module.PrintMessage($"Clicked {this.debugClickName} successfully.");
                }
                catch (ClickLib.ClickNotFoundError ex)
                {
                    this.module.PrintError(ex.Message);
                }
                catch (ClickLib.InvalidClickException ex)
                {
                    this.module.PrintError(ex.Message);
                }
                catch (Exception ex)
                {
                    this.module.PrintError(ex.Message);
                }
            }
        }

        #endregion

        private void UiBuilder_TextNodeButtons()
        {
            var style = ImGui.GetStyle();
            var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

            if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
            {
                var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                this.RootFolder.Children.Add(newNode);
                this.module.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
            {
                var newNode = new TextEntryNode { Enabled = true, Text = this.module.LastSeenDialogText };
                this.RootFolder.Children.Add(newNode);
                this.module.Configuration.Save();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
            {
                var newNode = new TextFolderNode { Name = "Untitled folder" };
                this.RootFolder.Children.Add(newNode);
                this.module.Configuration.Save();
            }

            var sb = new StringBuilder();
            sb.AppendLine("Enter into the input all or part of the text inside a dialog.");
            sb.AppendLine("For example: \"Teleport to \" for the teleport dialog.");
            sb.AppendLine();
            sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
            sb.AppendLine("As such: \"/Teleport to .*? for \\d+ gil\\?/\"");
            sb.AppendLine();
            sb.AppendLine("If it matches, the yes button (and checkbox if present) will be clicked.");
            sb.AppendLine();
            sb.AppendLine("Right click a line to view options.");
            sb.AppendLine("Double click an entry for quick enable/disable.");
            sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
            sb.AppendLine();
            sb.AppendLine("Currently supported text addons:");
            sb.AppendLine("  - SelectYesNo");
            sb.AppendLine();
            sb.AppendLine("Non-text addons are each listed separately in the lower config section.");

            ImGui.SameLine();
            ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

            ImGui.PopStyleVar(); // ItemSpacing
        }

        private void UiBuilder_TextNodes()
        {
            if (ImGui.CollapsingHeader("Text Entries"))
            {
                var root = this.module.Configuration.RootFolder;
                this.TextNodeDragDrop(root);

                if (root.Children.Count == 0)
                {
                    root.Children.Add(new TextEntryNode() { Enabled = false, Text = "Add some text here!" });
                    this.module.Configuration.Save();
                }

                foreach (var node in root.Children.ToArray())
                {
                    this.UiBuilder_DisplayTextNode(node);
                }
            }
        }

        private void UiBuilder_DisplayTextNode(INode node)
        {
            if (node is TextFolderNode folderNode)
            {
                this.DisplayTextFolderNode(folderNode);
            }
            else if (node is TextEntryNode macroNode)
            {
                this.UiBuilder_DisplayTextEntryNode(macroNode);
            }
        }

        private void UiBuilder_DisplayTextEntryNode(TextEntryNode node)
        {
            var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
            var validZone = !node.ZoneRestricted || (node.ZoneIsRegex && node.ZoneRegex != null) || !node.ZoneIsRegex;

            if (!node.Enabled && (!validRegex || !validZone))
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
            }
            else if (!node.Enabled)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
            }
            else if (!validRegex || !validZone)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
            }

            ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
            ImGui.TreePop();

            if (!node.Enabled || !validRegex || !validZone)
            {
                ImGui.PopStyleColor();
            }

            if (!validRegex && !validZone)
            {
                ImGuiEx.TextTooltip("Invalid Text and Zone Regex");
            }
            else if (!validRegex)
            {
                ImGuiEx.TextTooltip("Invalid Text Regex");
            }
            else if (!validZone)
            {
                ImGuiEx.TextTooltip("Invalid Zone Regex");
            }

            if (ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                {
                    node.Enabled = !node.Enabled;
                    this.module.Configuration.Save();
                    return;
                }
                else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var io = ImGui.GetIO();
                    if (io.KeyCtrl && io.KeyShift)
                    {
                        if (this.module.Configuration.TryFindParent(node, out var parent))
                        {
                            parent!.Children.Remove(node);
                            this.module.Configuration.Save();
                        }

                        return;
                    }
                    else
                    {
                        ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                    }
                }
            }

            this.TextNodePopup(node);
            this.TextNodeDragDrop(node);
        }

        private void DisplayTextFolderNode(TextFolderNode node)
        {
            var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}-tree");

            if (ImGui.IsItemHovered())
            {
                if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
                {
                    var io = ImGui.GetIO();
                    if (io.KeyCtrl && io.KeyShift)
                    {
                        if (this.module.Configuration.TryFindParent(node, out var parent))
                        {
                            parent!.Children.Remove(node);
                            this.module.Configuration.Save();
                        }

                        return;
                    }
                    else
                    {
                        ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                    }
                }
            }

            this.TextNodePopup(node);
            this.TextNodeDragDrop(node);

            if (expanded)
            {
                foreach (var childNode in node.Children.ToArray())
                {
                    this.UiBuilder_DisplayTextNode(childNode);
                }

                ImGui.TreePop();
            }
        }

        private void TextNodePopup(INode node)
        {
            var style = ImGui.GetStyle();
            var newItemSpacing = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);

            if (ImGui.BeginPopup($"{node.GetHashCode()}-popup"))
            {
                if (node is TextEntryNode entryNode)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                    var enabled = entryNode.Enabled;
                    if (ImGui.Checkbox("Enabled", ref enabled))
                    {
                        entryNode.Enabled = enabled;
                        this.module.Configuration.Save();
                    }

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt));
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (this.module.Configuration.TryFindParent(node, out var parentNode))
                        {
                            parentNode!.Children.Remove(node);
                            this.module.Configuration.Save();
                        }
                    }

                    var matchText = entryNode.Text;
                    if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        entryNode.Text = matchText;
                        this.module.Configuration.Save();
                    }

                    var zoneRestricted = entryNode.ZoneRestricted;
                    if (ImGui.Checkbox("Zone Restricted", ref zoneRestricted))
                    {
                        entryNode.ZoneRestricted = zoneRestricted;
                        this.module.Configuration.Save();
                    }

                    var searchWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.Search);
                    var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Search, "Zone List"))
                    {
                        this.module.OpenZoneListUi();
                    }

                    ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - newItemSpacing.X);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current zone"))
                    {
                        var currentID = DalamudBinding.ClientState.TerritoryType;
                        if (this.module.TerritoryNames.TryGetValue(currentID, out var zoneName))
                        {
                            entryNode.ZoneText = zoneName;
                        }
                        else
                        {
                            entryNode.ZoneText = "Could not find name";
                        }
                    }

                    ImGui.PopStyleVar(); // ItemSpacing

                    var zoneText = entryNode.ZoneText;
                    if (ImGui.InputText($"##{node.Name}-zoneText", ref zoneText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        entryNode.ZoneText = zoneText;
                        this.module.Configuration.Save();
                    }
                }

                if (node is TextFolderNode folderNode)
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                    if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add entry"))
                    {
                        var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                        this.module.Configuration.Save();
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
                    {
                        var newNode = new TextEntryNode() { Enabled = true, Text = this.module.LastSeenDialogText };
                        folderNode.Children.Add(newNode);
                        this.module.Configuration.Save();
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                    {
                        var newNode = new TextFolderNode { Name = "Untitled folder" };
                        folderNode.Children.Add(newNode);
                        this.module.Configuration.Save();
                    }

                    var trashWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);
                    ImGui.SameLine(ImGui.GetContentRegionMax().X - trashWidth);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (this.module.Configuration.TryFindParent(node, out var parentNode))
                        {
                            parentNode!.Children.Remove(node);
                            this.module.Configuration.Save();
                        }
                    }

                    ImGui.PopStyleVar(); // ItemSpacing

                    var folderName = folderNode.Name;
                    if (ImGui.InputText($"##{node.Name}-rename", ref folderName, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                    {
                        folderNode.Name = folderName;
                        this.module.Configuration.Save();
                    }
                }

                ImGui.EndPopup();
            }
        }

        private void TextNodeDragDrop(INode node)
        {
            if (node != this.module.Configuration.RootFolder && ImGui.BeginDragDropSource())
            {
                this.draggedNode = node;

                ImGui.Text(node.Name);
                ImGui.SetDragDropPayload("TextNodePayload", IntPtr.Zero, 0);
                ImGui.EndDragDropSource();
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("TextNodePayload");

                bool nullPtr;
                unsafe
                {
                    nullPtr = payload.NativePtr == null;
                }

                var targetNode = node;
                if (!nullPtr && payload.IsDelivery() && this.draggedNode != null)
                {
                    if (this.module.Configuration.TryFindParent(this.draggedNode, out var draggedNodeParent))
                    {
                        if (targetNode is TextFolderNode targetFolderNode)
                        {
                            draggedNodeParent!.Children.Remove(this.draggedNode);
                            targetFolderNode.Children.Add(this.draggedNode);
                            this.module.Configuration.Save();
                        }
                        else
                        {
                            if (this.module.Configuration.TryFindParent(targetNode, out var targetNodeParent))
                            {
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
                                this.module.Configuration.Save();
                            }
                            else
                            {
                                throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Could not find parent of node \"{this.draggedNode.Name}\"");
                    }

                    this.draggedNode = null;
                }

                ImGui.EndDragDropTarget();
            }
        }

        private void UiBuilder_ItemsWithoutText()
        {
            if (!ImGui.CollapsingHeader("Non-text Matching"))
            {
                return;
            }

            static void IndentedTextColored(Vector4 color, string text)
            {
                var indent = 27f * ImGuiHelpers.GlobalScale;
                ImGui.Indent(indent);
                ImGui.TextColored(color, text);
                ImGui.Unindent(indent);
            }

            var desynthDialog = this.module.Configuration.DesynthDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
            {
                this.module.Configuration.DesynthDialogEnabled = desynthDialog;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the Desynthesis menu confirmation.");

            var desynthBulkDialog = this.module.Configuration.DesynthBulkDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
            {
                this.module.Configuration.DesynthBulkDialogEnabled = desynthBulkDialog;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Check the bulk desynthesis button when using the SalvageDialog feature.");

            var materialize = this.module.Configuration.MaterializeDialogEnabled;
            if (ImGui.Checkbox("MaterializeDialog", ref materialize))
            {
                this.module.Configuration.MaterializeDialogEnabled = materialize;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the create new materia confirmation.");

            var materiaRetrieve = this.module.Configuration.MateriaRetrieveDialogEnabled;
            if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
            {
                this.module.Configuration.MateriaRetrieveDialogEnabled = materiaRetrieve;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the retrieve materia confirmation.");

            var itemInspection = this.module.Configuration.ItemInspectionResultEnabled;
            if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
            {
                this.module.Configuration.ItemInspectionResultEnabled = itemInspection;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Eureka/Bozja lockboxes, forgotten fragments, and more.\nWarning: this does not check if you are maxed on items.");

            var retainerTaskAsk = this.module.Configuration.RetainerTaskAskEnabled;
            if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
            {
                this.module.Configuration.RetainerTaskAskEnabled = retainerTaskAsk;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Skip the confirmation in the final dialog before sending out a retainer.");

            var retainerTaskResult = this.module.Configuration.RetainerTaskResultEnabled;
            if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
            {
                this.module.Configuration.RetainerTaskResultEnabled = retainerTaskResult;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically send a retainer on the same venture as before when receiving an item.");

            var grandCompanySupplyReward = this.module.Configuration.GrandCompanySupplyReward;
            if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
            {
                this.module.Configuration.GrandCompanySupplyReward = grandCompanySupplyReward;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Skip the confirmation when submitting Grand Company expert delivery items.");

            var shopCard = this.module.Configuration.ShopCardDialog;
            if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
            {
                this.module.Configuration.ShopCardDialog = shopCard;
                this.module.Configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically confirm selling Triple Triad cards in the saucer.");
        }
    }
}
