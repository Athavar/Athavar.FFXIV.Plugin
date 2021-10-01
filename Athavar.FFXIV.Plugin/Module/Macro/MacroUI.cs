using Athavar.FFXIV.Plugin;
using Dalamud.Interface;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SomethingNeedDoing
{
    // TODO timer ui

    internal class MacroUI : IDisposable
    {
        private readonly MacroModule plugin;
        private readonly Regex INCREMENTAL_NAME = new(@"(?<all> \((?<index>\d+)\))$", RegexOptions.Compiled);
        private readonly List<AddNodeOperation> AddNode = new();
        private readonly List<RemoveNodeOperation> RemoveNode = new();
        private INode? DraggedNode = null;
        private MacroNode? ActiveMacroNode = null;
        private ImFontPtr MonoFont;
        private ImFontPtr MonoFontJP;

        private struct AddNodeOperation
        {
            public INode Node;
            public FolderNode ParentNode;
            public int Index;
        }

        private struct RemoveNodeOperation
        {
            public INode Node;
            public FolderNode ParentNode;
        }

        public MacroUI(MacroModule plugin)
        {
            this.plugin = plugin;

            DalamudBinding.PluginInterface.UiBuilder.BuildFonts += UiBuilder_OnBuildFonts;
            DalamudBinding.PluginInterface.UiBuilder.RebuildFonts();
        }

        public void Dispose()
        {
            DalamudBinding.PluginInterface.UiBuilder.BuildFonts -= UiBuilder_OnBuildFonts;
            DalamudBinding.PluginInterface.UiBuilder.RebuildFonts();
        }

        public void UiBuilder_OnBuildFonts()
        {
            try
            {
                var fontData = plugin.ReadResourceFile("ProggyVectorRegular.ttf");
                var fontPtr = Marshal.AllocHGlobal(fontData.Length);
                Marshal.Copy(fontData, 0, fontPtr, fontData.Length);

                var fontDataJP = plugin.ReadResourceFile("NotoSansMonoCJKjp-Regular.otf");
                var fontPtrJP = Marshal.AllocHGlobal(fontDataJP.Length);
                Marshal.Copy(fontDataJP, 0, fontPtrJP, fontDataJP.Length);

                unsafe
                {
                    ImFontConfigPtr fontConfig = ImGuiNative.ImFontConfig_ImFontConfig();
                    fontConfig.MergeMode = true;
                    fontConfig.PixelSnapH = true;

                    MonoFont = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(fontPtr, fontData.Length, plugin.configuration.CustomFontSize, fontConfig);

                    // Interface.GlyphRangesJapanese is internal, should be public
                    // Once it is, our file can be deleted
                    var japaneseRangeHandle = GCHandle.Alloc(GlyphRangesJapanese.GlyphRanges, GCHandleType.Pinned);
                    MonoFontJP = ImGui.GetIO().Fonts.AddFontFromMemoryTTF(fontPtrJP, fontDataJP.Length, plugin.configuration.CustomFontSize, fontConfig, japaneseRangeHandle.AddrOfPinnedObject());
                    japaneseRangeHandle.Free();
                }
            }
            catch (Exception e)
            {
                PluginLog.LogError("Error during rebuild of Fonts. {0}: {1}", e.Message, e.StackTrace ?? string.Empty);
            }
        }

        public void UiBuilder_OnBuildUi()
        {
            using var raii = new ImGuiRaii();
            if (!raii.Begin(() => ImGui.BeginTabItem("Macro"), ImGui.EndTabItem))
                return;

            ImGui.Columns(2);

            DisplayNode(plugin.configuration.RootFolder);

            ImGui.NextColumn();

            DisplayRunningMacros();

            DisplayMacroEdit();

            ImGui.Columns(1);

            ResolveAddRemoveNodes();
        }

        #region node tree

        private void DisplayNode(INode node)
        {
            if (node is FolderNode folderNode)
                DisplayFolderNode(folderNode);
            else if (node is MacroNode macroNode)
                DisplayMacroNode(macroNode);
        }

        private void DisplayMacroNode(MacroNode node)
        {
            var flags = ImGuiTreeNodeFlags.Leaf;
            if (node == ActiveMacroNode)
                flags |= ImGuiTreeNodeFlags.Selected;

            ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", flags);

            NodePopup(node);
            NodeDragDrop(node);

            if (ImGui.IsItemClicked())
            {
                ActiveMacroNode = node;
            }

            ImGui.TreePop();
        }

        private void DisplayFolderNode(FolderNode node)
        {
            if (node == plugin.configuration.RootFolder)
            {
                ImGui.SetNextItemOpen(true, ImGuiCond.FirstUseEver);
            }
            var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree");

            NodePopup(node);
            NodeDragDrop(node);

            if (expanded)
            {
                foreach (var childNode in node.Children)
                    DisplayNode(childNode);
                ImGui.TreePop();
            }
        }

        private string GetUniqueNodeName(string name)
        {
            var nodeNames = plugin.configuration.GetAllNodes().Select(node => node.Name).ToList();
            while (nodeNames.Contains(name))
            {
                Match match = INCREMENTAL_NAME.Match(name);
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
                if (ImGui.InputText($"##{node.Name}-rename", ref name, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    node.Name = GetUniqueNodeName(name);
                    plugin.SaveConfiguration();
                }

                if (node is MacroNode macroNode)
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
                    {
                        plugin.MacroManager.RunMacro(macroNode);
                    }
                }

                if (node is FolderNode folderNode)
                {
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add macro"))
                    {
                        var newNode = new MacroNode { Name = GetUniqueNodeName("Untitled macro") };
                        AddNode.Add(new AddNodeOperation { Node = newNode, ParentNode = folderNode, Index = -1 });
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                    {
                        var newNode = new FolderNode { Name = GetUniqueNodeName("Untitled folder") };
                        AddNode.Add(new AddNodeOperation { Node = newNode, ParentNode = folderNode, Index = -1 });
                    }
                }

                if (node != plugin.configuration.RootFolder)
                {
                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Copy, "Copy Name"))
                    {
                        ImGui.SetClipboardText(node.Name);
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                    {
                        if (plugin.configuration.TryFindParent(node, out var parentNode) && parentNode is not null)
                        {
                            RemoveNode.Add(new RemoveNodeOperation { Node = node, ParentNode = parentNode });
                        }
                    }
                    ImGui.SameLine();
                }

                ImGui.EndPopup();
            }
        }

        private void NodeDragDrop(INode node)
        {
            if (node != plugin.configuration.RootFolder)
            {
                if (ImGui.BeginDragDropSource())
                {
                    DraggedNode = node;
                    ImGui.Text(node.Name);
                    ImGui.SetDragDropPayload("NodePayload", IntPtr.Zero, 0);
                    ImGui.EndDragDropSource();
                }
            }

            if (ImGui.BeginDragDropTarget())
            {
                var payload = ImGui.AcceptDragDropPayload("NodePayload");

                bool nullPtr;
                unsafe { nullPtr = payload.NativePtr == null; }

                var targetNode = node;
                if (!nullPtr && payload.IsDelivery() && DraggedNode != null)
                {
                    if (plugin.configuration.TryFindParent(DraggedNode, out var draggedNodeParent) && draggedNodeParent is not null)
                    {
                        if (targetNode is FolderNode targetFolderNode)
                        {
                            AddNode.Add(new AddNodeOperation { Node = DraggedNode, ParentNode = targetFolderNode, Index = -1 });
                            RemoveNode.Add(new RemoveNodeOperation { Node = DraggedNode, ParentNode = draggedNodeParent });
                        }
                        else
                        {
                            if (plugin.configuration.TryFindParent(targetNode, out var targetNodeParent) && targetNodeParent is not null)
                            {
                                var targetNodeIndex = targetNodeParent.Children.IndexOf(targetNode);
                                if (targetNodeParent == draggedNodeParent)
                                {
                                    var draggedNodeIndex = targetNodeParent.Children.IndexOf(DraggedNode);
                                    if (draggedNodeIndex < targetNodeIndex)
                                    {
                                        targetNodeIndex -= 1;
                                    }
                                }
                                AddNode.Add(new AddNodeOperation { Node = DraggedNode, ParentNode = targetNodeParent, Index = targetNodeIndex });
                                RemoveNode.Add(new RemoveNodeOperation { Node = DraggedNode, ParentNode = draggedNodeParent });
                            }
                            else
                            {
                                throw new Exception($"Could not find parent of node \"{targetNode.Name}\"");
                            }
                        }
                    }
                    else
                    {
                        throw new Exception($"Could not find parent of node \"{DraggedNode.Name}\"");
                    }
                    DraggedNode = null;
                }
                ImGui.EndDragDropTarget();
            }
        }

        #endregion

        #region running macros

        private void DisplayRunningMacros()
        {
            ImGui.Text("Macro Queue");

            var state = Enum.GetName(typeof(LoopState), plugin.MacroManager.LoopState);

            Vector4 buttonCol;
            unsafe { buttonCol = *ImGui.GetStyleColorVec4(ImGuiCol.Button); }
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonCol);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonCol);
            ImGui.Button($"{state}##LoopState", new Vector2(100, 0));
            ImGui.PopStyleColor();
            ImGui.PopStyleColor();

            if (plugin.MacroManager.LoopState == LoopState.NotLoggedIn) { /* Nothing to do */ }
            else if (plugin.MacroManager.LoopState == LoopState.Stopped) { /* Nothing to do */ }
            else if (plugin.MacroManager.LoopState == LoopState.Waiting) { /* Nothing to do */ }
            else if (plugin.MacroManager.LoopState == LoopState.Paused)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Resume")) { plugin.MacroManager.Resume(); }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Clear")) { plugin.MacroManager.Clear(); }
            }
            else if (plugin.MacroManager.LoopState == LoopState.Running)
            {
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Pause, "Pause")) { plugin.MacroManager.Pause(); }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Clear")) { plugin.MacroManager.Clear(); }
            }

            ImGui.PushItemWidth(-1);

            var style = ImGui.GetStyle();
            var runningHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 3) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 2);
            if (ImGui.BeginListBox("##running-macros", new Vector2(-1, runningHeight)))
            {
                var macroStatus = plugin.MacroManager.MacroStatus;
                for (int i = 0; i < macroStatus.Length; i++)
                {
                    var (name, stepIndex) = macroStatus[i];
                    string text = name;
                    if (i == 0 || stepIndex > 1)
                        text += $" (step {stepIndex})";
                    ImGui.Selectable($"{text}##{Guid.NewGuid()}", i == 0);
                }
                ImGui.EndListBox();
            }

            var contentHeight = (ImGui.CalcTextSize("CalcTextSize").Y * ImGuiHelpers.GlobalScale * 5) + (style.FramePadding.Y * 2) + (style.ItemSpacing.Y * 4);
            var macroContent = plugin.MacroManager.CurrentMacroContent();
            if (ImGui.BeginListBox("##current-macro", new Vector2(-1, contentHeight)))
            {
                var stepIndex = plugin.MacroManager.CurrentMacroStep();
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
            var node = ActiveMacroNode;
            if (node is null)
                return;

            ImGui.Text("Macro Editor");

            if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Run"))
            {
                plugin.MacroManager.RunMacro(node);
            }

            ImGui.SameLine();
            ImGuiEx.IconButton(FontAwesomeIcon.TextHeight, "Text Size");
            if (ImGui.BeginPopupContextItem("font-editor", ImGuiPopupFlags.None))
            {
                ImGui.BeginPopup("font-editor");
                var fontSize = (int)plugin.configuration.CustomFontSize;
                if (ImGui.SliderInt("Font Size", ref fontSize, 10, 30))
                {
                    plugin.configuration.CustomFontSize = fontSize;
                    DalamudBinding.PluginInterface.UiBuilder.RebuildFonts();
                    plugin.SaveConfiguration();
                }
                ImGui.EndPopup();
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
                    text = text.Remove(index, 1).Insert(index, "\r\n");

                node.Contents = text;
                plugin.SaveConfiguration();
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.TimesCircle, "Close"))
            {
                ActiveMacroNode = null;
            }

            ImGui.PushItemWidth(-1);
            ImGui.PushFont(MonoFont);
            ImGui.PushFont(MonoFontJP);

            var contents = node.Contents;
            if (ImGui.InputTextMultiline($"##{node.Name}-editor", ref contents, 100_000, new Vector2(-1, -1)))
            {
                node.Contents = contents;
                plugin.SaveConfiguration();
            }

            ImGui.PopFont();
            ImGui.PopFont();
            ImGui.PopItemWidth();
        }

        #endregion

        private void ResolveAddRemoveNodes()
        {
            if (AddNode.Count > 0 || RemoveNode.Count > 0)
            {
                if (RemoveNode.Count > 0)
                {
                    foreach (var inst in RemoveNode)
                    {
                        if (ActiveMacroNode == inst.Node)
                            ActiveMacroNode = null;

                        inst.ParentNode.Children.Remove(inst.Node);
                    }
                    RemoveNode.Clear();
                }

                if (AddNode.Count > 0)
                {
                    foreach (var inst in AddNode)
                    {
                        if (inst.Index < 0)
                            inst.ParentNode.Children.Add(inst.Node);
                        else
                            inst.ParentNode.Children.Insert(inst.Index, inst.Node);
                    }
                    AddNode.Clear();
                }
                plugin.SaveConfiguration();
            }
        }
    }

    internal static class ImGuiEx
    {
        public static bool IconButton(FontAwesomeIcon icon) => IconButton(icon);

        public static bool IconButton(FontAwesomeIcon icon, string tooltip)
        {
            ImGui.PushFont(UiBuilder.IconFont);
            var result = ImGui.Button($"{icon.ToIconString()}##{icon.ToIconString()}-{tooltip}");
            ImGui.PopFont();

            if (tooltip != null)
                TextTooltip(tooltip);

            return result;
        }

        public static void TextTooltip(string text)
        {
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(text);
                ImGui.EndTooltip();
            }
        }

        #region rotation

        private static int rotation_start_index;

        public static Vector2 Min(Vector2 lhs, Vector2 rhs) => new(lhs.X < rhs.X ? lhs.X : rhs.X, lhs.Y < rhs.Y ? lhs.Y : rhs.Y);

        public static Vector2 Max(Vector2 lhs, Vector2 rhs) => new(lhs.X >= rhs.X ? lhs.X : rhs.X, lhs.Y >= rhs.Y ? lhs.Y : rhs.Y);

        private static Vector2 Rotate(Vector2 v, float cos_a, float sin_a) => new((v.X * cos_a) - (v.Y * sin_a), (v.X * sin_a) + (v.Y * cos_a));

        public static void RotateStart()
        {
            rotation_start_index = ImGui.GetWindowDrawList().VtxBuffer.Size;
        }

        public static void RotateEnd(double rad) => RotateEnd(rad, RotationCenter());

        public static void RotateEnd(double rad, Vector2 center)
        {
            var sin = (float)Math.Sin(rad);
            var cos = (float)Math.Cos(rad);
            center = Rotate(center, sin, cos) - center;

            var buf = ImGui.GetWindowDrawList().VtxBuffer;
            for (int i = rotation_start_index; i < buf.Size; i++)
                buf[i].pos = Rotate(buf[i].pos, sin, cos) - center;
        }

        private static Vector2 RotationCenter()
        {
            var l = new Vector2(float.MaxValue, float.MaxValue);
            var u = new Vector2(float.MinValue, float.MinValue);

            var buf = ImGui.GetWindowDrawList().VtxBuffer;
            for (int i = rotation_start_index; i < buf.Size; i++)
            {
                l = Min(l, buf[i].pos);
                u = Max(u, buf[i].pos);
            }

            return new Vector2((l.X + u.X) / 2, (l.Y + u.Y) / 2);
        }

        #endregion
    }
}
