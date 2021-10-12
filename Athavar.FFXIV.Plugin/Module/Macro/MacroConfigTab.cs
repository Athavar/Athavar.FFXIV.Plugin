namespace SomethingNeedDoing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using Athavar.FFXIV.Plugin;
    using Dalamud.Interface;
    using Dalamud.Logging;
    using ImGuiNET;
    using static SomethingNeedDoing.MacroManager;

    /// <summary>
    /// UI Tab for macro execution.
    /// </summary>
    internal class MacroConfigTab
    {
        private readonly MacroModule plugin;
        private readonly Regex incrementalName = new(@"(?<all> \((?<index>\d+)\))$", RegexOptions.Compiled);
        private readonly List<AddNodeOperation> addNode = new();
        private readonly List<RemoveNodeOperation> removeNode = new();

        private INode? draggedNode = null;
        private MacroNode? activeMacroNode = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroConfigTab"/> class.
        /// </summary>
        /// <param name="plugin">The <see cref="MacroModule"/>.</param>
        public MacroConfigTab(MacroModule plugin)
        {
            this.plugin = plugin;
        }

        public void DrawTab()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Macro"), ImGui.EndTabItem))
            {
                return;
            }

            ImGui.Columns(2);

            this.DisplayNode(this.plugin.Configuration.RootFolder);

            ImGui.NextColumn();

            this.DisplayRunningMacros();

            this.DisplayMacroEdit();

            ImGui.Columns(1);

            this.ResolveAddRemoveNodes();
        }

        #region node tree

        private void DisplayNode(INode node)
        {
            ImGui.PushID(node.Name);

            if (node is FolderNode folderNode)
            {
                this.DisplayFolderNode(folderNode);
            }
            else if (node is MacroNode macroNode)
            {
                this.DisplayMacroNode(macroNode);
            }

            ImGui.PopID();
        }

        private void DisplayMacroNode(MacroNode node)
        {
            var flags = ImGuiTreeNodeFlags.Leaf;
            if (node == this.activeMacroNode)
            {
                flags |= ImGuiTreeNodeFlags.Selected;
            }

            ImGui.TreeNodeEx($"{node.Name}##tree", flags);

            this.NodePopup(node);
            this.NodeDragDrop(node);

            if (ImGui.IsItemClicked())
            {
                this.activeMacroNode = node;
            }

            ImGui.TreePop();
        }

        private void DisplayFolderNode(FolderNode node)
        {
            if (node == this.plugin.Configuration.RootFolder)
            {
                ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
            }

            var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree");

            this.NodePopup(node);
            this.NodeDragDrop(node);

            if (expanded)
            {
                foreach (var childNode in node.Children)
                {
                    this.DisplayNode(childNode);
                }

                ImGui.TreePop();
            }
        }

        private string GetUniqueNodeName(string name)
        {
            var nodeNames = this.plugin.Configuration.GetAllNodes().Select(node => node.Name).ToList();
            while (nodeNames.Contains(name))
            {
                Match match = this.incrementalName.Match(name);
                if (match.Success)
                {
                    var all = match.Groups["all"].Value;
                    var index = int.Parse(match.Groups["index"].Value);
                    name = name.Substring(0, name.Length - all.Length);
                    name = $"{name} ({index + 1})";
                }
                else
                {
                    name = $"{name} (1)";
                }
            }

            return name.Trim();
        }

        private void NodePopup(INode node)
        {
            if (ImGui.BeginPopupContextItem($"##{node.Name}-popup"))
            {
                var name = node.Name;
                if (ImGui.InputText($"##rename", ref name, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    node.Name = this.GetUniqueNodeName(name);
                    this.plugin.SaveConfiguration();
                }

                if (node is MacroNode macroNode)
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
                    {
                        this.plugin.MacroManager.RunMacro(macroNode);
                    }
                }

                if (node is FolderNode folderNode)
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add macro"))
                    {
                        var newNode = new MacroNode { Name = this.GetUniqueNodeName("Untitled macro") };
                        this.addNode.Add(new AddNodeOperation { Node = newNode, ParentNode = folderNode, Index = -1 });
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                    {
                        var newNode = new FolderNode { Name = this.GetUniqueNodeName("Untitled folder") };
                        this.addNode.Add(new AddNodeOperation { Node = newNode, ParentNode = folderNode, Index = -1 });
                    }
                }

                if (node != this.plugin.Configuration.RootFolder)
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Copy, "Copy Name"))
                    {
                        ImGui.SetClipboardText(node.Name);
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (this.plugin.Configuration.TryFindParent(node, out var parentNode))
                        {
                            this.removeNode.Add(new RemoveNodeOperation { Node = node, ParentNode = parentNode! });
                        }
                    }

                    ImGui.SameLine();
                }

                ImGui.EndPopup();
            }
        }

        private void NodeDragDrop(INode node)
        {
            if (node != this.plugin.Configuration.RootFolder)
            {
                if (ImGui.BeginDragDropSource())
                {
                    this.draggedNode = node;
                    ImGui.Text(node.Name);
                    ImGui.SetDragDropPayload("NodePayload", IntPtr.Zero, 0);
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
                    if (this.plugin.Configuration.TryFindParent(this.draggedNode, out var draggedNodeParent))
                    {
                        if (targetNode is FolderNode targetFolderNode)
                        {
                            this.addNode.Add(new AddNodeOperation { Node = this.draggedNode, ParentNode = targetFolderNode, Index = -1 });
                            this.removeNode.Add(new RemoveNodeOperation { Node = this.draggedNode, ParentNode = draggedNodeParent! });
                        }
                        else
                        {
                            if (this.plugin.Configuration.TryFindParent(targetNode, out var targetNodeParent))
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

                                this.addNode.Add(new AddNodeOperation { Node = this.draggedNode, ParentNode = targetNodeParent, Index = targetNodeIndex });
                                this.removeNode.Add(new RemoveNodeOperation { Node = this.draggedNode, ParentNode = draggedNodeParent! });
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

        #endregion

        #region running macros

        private void DisplayRunningMacros()
        {
            ImGui.Text("Macro Queue");

            var state = Enum.GetName(this.plugin.MacroManager.State);

            Vector4 buttonCol;
            unsafe
            {
                buttonCol = *ImGui.GetStyleColorVec4(ImGuiCol.Button);
            }

            ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonCol);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonCol);
            ImGui.Button($"{state}##LoopState", new Vector2(100, 0));
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            if (this.plugin.MacroManager.State == LoopState.NotLoggedIn)
            { /* Nothing to do */
            }
            else if (this.plugin.MacroManager.State == LoopState.Stopped)
            { /* Nothing to do */
            }
            else if (this.plugin.MacroManager.State == LoopState.Waiting)
            { /* Nothing to do */
            }
            else if (this.plugin.MacroManager.State == LoopState.Paused)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Resume"))
                {
                    this.plugin.MacroManager.Resume();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Clear"))
                {
                    this.plugin.MacroManager.Clear();
                }
            }
            else if (this.plugin.MacroManager.State == LoopState.Running || this.plugin.MacroManager.State == LoopState.Cancel)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Pause, "Pause"))
                {
                    this.plugin.MacroManager.Pause();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Stop, "Cancel"))
                {
                    this.plugin.MacroManager.Cancel();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Clear"))
                {
                    this.plugin.MacroManager.Clear();
                }
            }

            ImGui.PushItemWidth(-1);

            var style = ImGui.GetStyle();
            var runningHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 3) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 2);
            if (ImGui.BeginListBox("##running-macros", new Vector2(-1, runningHeight)))
            {
                var macroStatus = this.plugin.MacroManager.MacroStatus;
                for (int i = 0; i < macroStatus.Length; i++)
                {
                    var (name, stepIndex) = macroStatus[i];
                    string text = name;
                    if (i == 0 || stepIndex > 1)
                    {
                        text += $" (step {stepIndex})";
                    }

                    ImGui.Selectable($"{text}##{Guid.NewGuid()}", i == 0);
                }

                ImGui.EndListBox();
            }

            var contentHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 5) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 4);
            var macroContent = this.plugin.MacroManager.CurrentMacroContent();
            if (ImGui.BeginListBox("##current-macro", new Vector2(-1, contentHeight)))
            {
                var stepIndex = this.plugin.MacroManager.CurrentMacroStep();
                if (stepIndex == -1)
                {
                    ImGui.Selectable("Looping", true);
                }
                else
                {
                    for (int i = stepIndex; i < macroContent.Length; i++)
                    {
                        var step = macroContent[i];
                        var isCurrentStep = i == stepIndex;
                        ImGui.Selectable(step, isCurrentStep);
                    }
                }

                ImGui.EndListBox();
            }

            ImGui.PopItemWidth();
        }

        #endregion

        #region macro edit

        private void DisplayMacroEdit()
        {
            var node = this.activeMacroNode;
            if (node is null)
            {
                return;
            }

            ImGui.Text("Macro Editor");

            if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
            {
                this.plugin.MacroManager.RunMacro(node);
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.FileImport, "Import from clipboard"))
            {
                var text = ImGui.GetClipboardText();

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

                node.Contents = text;
                this.plugin.SaveConfiguration();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.TimesCircle, "Close"))
            {
                this.activeMacroNode = null;
            }

            ImGui.PushItemWidth(-1);
            ImGui.PushFont(UiBuilder.MonoFont);

            var contents = node.Contents;
            if (ImGui.InputTextMultiline($"##{node.Name}-editor", ref contents, 100_000, new Vector2(-1, -1)))
            {
                node.Contents = contents;
                this.plugin.SaveConfiguration();
            }

            ImGui.PopFont();
            ImGui.PopItemWidth();
        }

        #endregion

        private void ResolveAddRemoveNodes()
        {
            if (this.addNode.Count > 0 || this.removeNode.Count > 0)
            {
                if (this.removeNode.Count > 0)
                {
                    foreach (var inst in this.removeNode)
                    {
                        if (this.activeMacroNode == inst.Node)
                        {
                            this.activeMacroNode = null;
                        }

                        inst.ParentNode.Children.Remove(inst.Node);
                    }

                    this.removeNode.Clear();
                }

                if (this.addNode.Count > 0)
                {
                    foreach (var inst in this.addNode)
                    {
                        if (inst.Index < 0)
                        {
                            inst.ParentNode.Children.Add(inst.Node);
                        }
                        else
                        {
                            inst.ParentNode.Children.Insert(inst.Index, inst.Node);
                        }
                    }

                    this.addNode.Clear();
                }

                this.plugin.SaveConfiguration();
            }
        }

        private struct AddNodeOperation
        {
            public INode Node { get; init; }

            public FolderNode ParentNode { get; init; }

            public int Index { get; init; }
        }

        private struct RemoveNodeOperation
        {
            public INode Node { get; init; }

            public FolderNode ParentNode { get; init; }
        }

        private static class ImGuiEx
        {
            public static bool IconButton(FontAwesomeIcon icon) => IconButton(icon);

            public static bool IconButton(FontAwesomeIcon icon, string tooltip)
            {
                ImGui.PushFont(UiBuilder.IconFont);
                var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
                ImGui.PopFont();

                if (tooltip != null)
                {
                    TextTooltip(tooltip);
                }

                return result;
            }

            /// <summary>
            /// Show a simple text tooltip if hovered.
            /// </summary>
            /// <param name="text">Text to display.</param>
            public static void TextTooltip(string text)
            {
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(text);
                    ImGui.EndTooltip();
                }
            }
        }
    }
}
