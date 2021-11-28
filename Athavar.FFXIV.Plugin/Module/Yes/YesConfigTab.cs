// <copyright file="YesConfigTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes;

using System;
using System.Numerics;
using System.Text;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Interface;
using ImGuiNET;

/// <summary>
///     Yes Module configuration Tab.
/// </summary>
internal class YesConfigTab
{
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly YesConfiguration configuration;
    private readonly ZoneListWindow zoneListWindow;
    private readonly Vector4 shadedColor = new(0.68f, 0.68f, 0.68f, 1.0f);

    private YesModule module = null!;
    private INode? draggedNode;
    private string debugClickName = string.Empty;

    public YesConfigTab(IDalamudServices dalamudServices, IChatManager chatManager, Configuration configuration, ZoneListWindow zoneListWindow)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.configuration = configuration.Yes!;
        this.zoneListWindow = zoneListWindow;
    }

    private TextFolderNode RootFolder => this.configuration.RootFolder;

    private TextFolderNode ListRootFolder => this.configuration.ListRootFolder;

    private TextFolderNode TalkRootFolder => this.configuration.TalkRootFolder;

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

        var enabled = this.configuration.Enabled;
        if (ImGui.Checkbox("Enabled", ref enabled))
        {
            this.configuration.Enabled = enabled;
            this.configuration.Save();
        }

        if (ImGui.BeginTabBar("Settings"))
        {
            this.DisplayTextOptions();
            this.DisplayListOptions();
            this.DisplayTalkOptions();
            this.DisplayBotherOptions();

            ImGui.EndTabBar();
        }
    }

    /// <summary>
    ///     Setup the <see cref="YesConfigTab" />.
    /// </summary>
    /// <param name="m">The <see cref="YesModule" />.</param>
    internal void Setup(YesModule m)
    {
        this.module = m;
        this.zoneListWindow.Setup(m);
    }

    private void UiBuilder_TestButton()
    {
        ImGui.InputText("ClickName", ref this.debugClickName, 100);
        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.Check, "Submit"))
        {
            try
            {
                this.debugClickName ??= string.Empty;
                Click.SendClick(this.debugClickName.Trim());
                this.chatManager.PrintMessage($"Clicked {this.debugClickName} successfully.");
            }
            catch (ClickNotFoundError ex)
            {
                this.chatManager.PrintError(ex.Message);
            }
            catch (InvalidClickException ex)
            {
                this.chatManager.PrintError(ex.Message);
            }
            catch (Exception ex)
            {
                this.chatManager.PrintError(ex.Message);
            }
        }
    }

    /* ==================================================================================================== */

    private void DisplayTextOptions()
    {
        if (!ImGui.BeginTabItem("YesNo"))
        {
            return;
        }

        ImGui.PushID("TextOptions");

        this.DisplayTextButtons();
        this.DisplayTextNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayListOptions()
    {
        if (!ImGui.BeginTabItem("Lists"))
        {
            return;
        }

        ImGui.PushID("ListOptions");

        this.DisplayListButtons();
        this.DisplayListNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayTalkOptions()
    {
        if (!ImGui.BeginTabItem("Talk"))
        {
            return;
        }

        ImGui.PushID("TalkOptions");

        this.DisplayTalkButtons();
        this.DisplayTalkNodes();

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    private void DisplayBotherOptions()
    {
        if (!ImGui.BeginTabItem("Bothers"))
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

        ImGui.PushID("BotherOptions");

        // SalvageDialog
        {
            var desynthDialog = this.configuration.DesynthDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
            {
                this.configuration.DesynthDialogEnabled = desynthDialog;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the Desynthesis menu confirmation.");
        }

        // SalvageDialog (Bulk)
        {
            var desynthBulkDialog = this.configuration.DesynthBulkDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
            {
                this.configuration.DesynthBulkDialogEnabled = desynthBulkDialog;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Check the bulk desynthesis button when using the SalvageDialog feature.");
        }

        // MaterializeDialog
        {
            var materialize = this.configuration.MaterializeDialogEnabled;
            if (ImGui.Checkbox("MaterializeDialog", ref materialize))
            {
                this.configuration.MaterializeDialogEnabled = materialize;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the create new materia confirmation.");
        }

        // MateriaRetrieveDialog
        {
            var materiaRetrieve = this.configuration.MateriaRetrieveDialogEnabled;
            if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
            {
                this.configuration.MateriaRetrieveDialogEnabled = materiaRetrieve;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Remove the retrieve materia confirmation.");
        }

        // ItemInspectionResult
        {
            var itemInspection = this.configuration.ItemInspectionResultEnabled;
            if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
            {
                this.configuration.ItemInspectionResultEnabled = itemInspection;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Eureka/Bozja lockboxes, forgotten fragments, and more.\nWarning: this does not check if you are maxed on items.");

            IndentedTextColored(this.shadedColor, "Rate limiter (pause after N items)");
            ImGui.SameLine();

            ImGui.PushItemWidth(100f * ImGuiHelpers.GlobalScale);
            var itemInspectionResultLimiter = this.configuration.ItemInspectionResultRateLimiter;
            if (ImGui.InputInt("###itemInspectionResultRateLimiter", ref itemInspectionResultLimiter))
            {
                if (itemInspectionResultLimiter < 0)
                {
                    itemInspectionResultLimiter = 0;
                }
                else
                {
                    this.configuration.ItemInspectionResultRateLimiter = itemInspectionResultLimiter;
                    this.configuration.Save();
                }
            }
        }

        // RetainerTaskAsk
        {
            var retainerTaskAsk = this.configuration.RetainerTaskAskEnabled;
            if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
            {
                this.configuration.RetainerTaskAskEnabled = retainerTaskAsk;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Skip the confirmation in the final dialog before sending out a retainer.");
        }

        // RetainerTaskResult
        {
            var retainerTaskResult = this.configuration.RetainerTaskResultEnabled;
            if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
            {
                this.configuration.RetainerTaskResultEnabled = retainerTaskResult;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically send a retainer on the same venture as before when receiving an item.");
        }

        // GrandCompanySupplyReward
        {
            var grandCompanySupplyReward = this.configuration.GrandCompanySupplyReward;
            if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
            {
                this.configuration.GrandCompanySupplyReward = grandCompanySupplyReward;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Skip the confirmation when submitting Grand Company expert delivery items.");
        }

        // ShopCardDialog
        {
            var shopCard = this.configuration.ShopCardDialog;
            if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
            {
                this.configuration.ShopCardDialog = shopCard;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically confirm selling Triple Triad cards in the saucer.");
        }

        // JournalResultComplete
        {
            var journalResultComplete = this.configuration.JournalResultCompleteEnabled;
            if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
            {
                this.configuration.JournalResultCompleteEnabled = journalResultComplete;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically confirm quest reward acceptance when there is nothing to choose.");
        }

        // ContentFinderConfirm
        {
            var contentsFinderConfirm = this.configuration.ContentsFinderConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderConfirm", ref contentsFinderConfirm))
            {
                this.configuration.ContentsFinderConfirmEnabled = contentsFinderConfirm;
                this.configuration.Save();
            }

            IndentedTextColored(this.shadedColor, "Automatically commence duties when ready.");
        }

        ImGui.PopID();

        ImGui.EndTabItem();
    }

    // ====================================================================================================
    private void DisplayTextButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
            this.RootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
        {
            var newNode = new TextEntryNode { Enabled = true, Text = this.module.LastSeenDialogText };
            this.RootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            this.RootFolder.Children.Add(newNode);
            this.configuration.Save();
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

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayTextNodes()
    {
        var root = this.RootFolder;
        this.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new TextEntryNode { Enabled = false, Text = "Add some text here!" });
            this.configuration.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            this.DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================
    private void DisplayListButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
            this.ListRootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last selected as new entry"))
        {
            var newNode = new ListEntryNode { Enabled = true, Text = this.module.LastSeenListSelection, TargetRestricted = true, TargetText = this.module.LastSeenListTarget };
            this.ListRootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            this.ListRootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the text inside a line in a list dialog.");
        sb.AppendLine("For example: \"Purchase a Mini Cactpot ticket\" in the Gold Saucer.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/Purchase a .*? ticket/\"");
        sb.AppendLine();
        sb.AppendLine("If any line in the list matches, then that line will be chosen.");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - SelectString");
        sb.AppendLine("  - SelectIconString");

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayListNodes()
    {
        var root = this.ListRootFolder;
        this.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new ListEntryNode { Enabled = false, Text = "Add some text here!" });
            this.configuration.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            this.DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================
    private void DisplayTalkButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add new entry"))
        {
            var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
            this.TalkRootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add current target as a new entry"))
        {
            var target = this.dalamudServices.TargetManager.Target;
            var targetName = this.module is null
                ? string.Empty
                : this.module.LastSeenTalkTarget = target != null
                    ? this.module.GetSeStringText(target.Name)
                    : string.Empty;

            var newNode = new TalkEntryNode { Enabled = true, TargetText = targetName };
            this.TalkRootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        ImGui.SameLine();
        if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
        {
            var newNode = new TextFolderNode { Name = "Untitled folder" };
            this.TalkRootFolder.Children.Add(newNode);
            this.configuration.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the selected target name while in a talk dialog.");
        sb.AppendLine("For example: \"Moyce\" in the Crystarium.");
        sb.AppendLine();
        sb.AppendLine("Alternatively, wrap your text in forward slashes to use as a regex.");
        sb.AppendLine("As such: \"/(Moyce|Eirikur)/\"");
        sb.AppendLine();
        sb.AppendLine("Right click a line to view options.");
        sb.AppendLine("Double click an entry for quick enable/disable.");
        sb.AppendLine("Ctrl-Shift right click a line to delete it and any children.");
        sb.AppendLine();
        sb.AppendLine("Currently supported list addons:");
        sb.AppendLine("  - Talk");

        ImGui.SameLine();
        ImGuiEx.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    private void DisplayTalkNodes()
    {
        var root = this.TalkRootFolder;
        this.TextNodeDragDrop(root);

        if (root.Children.Count == 0)
        {
            root.Children.Add(new TalkEntryNode { Enabled = false, TargetText = "Add some text here!" });
            this.configuration.Save();
        }

        foreach (var node in root.Children.ToArray())
        {
            this.DisplayTextNode(node, root);
        }
    }

    // ====================================================================================================
    private void DisplayTextNode(INode node, TextFolderNode rootNode)
    {
        if (node is TextFolderNode folderNode)
        {
            this.DisplayFolderNode(folderNode, rootNode);
        }
        else if (node is TextEntryNode textNode)
        {
            this.DisplayTextEntryNode(textNode);
        }
        else if (node is ListEntryNode listNode)
        {
            this.DisplayListEntryNode(listNode);
        }
        else if (node is TalkEntryNode talkNode)
        {
            this.DisplayTalkEntryNode(talkNode);
        }
    }

    private void DisplayTextEntryNode(TextEntryNode node)
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
                this.configuration.Save();
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node);
        this.TextNodeDragDrop(node);
    }

    private void DisplayListEntryNode(ListEntryNode node)
    {
        var validRegex = (node.IsTextRegex && node.TextRegex != null) || !node.IsTextRegex;
        var validTarget = !node.TargetRestricted || (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

        if (!node.Enabled && (!validRegex || !validTarget))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        }
        else if (!node.Enabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        }
        else if (!validRegex || !validTarget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
        }

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex || !validTarget)
        {
            ImGui.PopStyleColor();
        }

        if (!validRegex && !validTarget)
        {
            ImGuiEx.TextTooltip("Invalid Text and Target Regex");
        }
        else if (!validRegex)
        {
            ImGuiEx.TextTooltip("Invalid Text Regex");
        }
        else if (!validTarget)
        {
            ImGuiEx.TextTooltip("Invalid Target Regex");
        }

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                this.configuration.Save();
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node);
        this.TextNodeDragDrop(node);
    }

    private void DisplayTalkEntryNode(TalkEntryNode node)
    {
        var validTarget = (node.TargetIsRegex && node.TargetRegex != null) || !node.TargetIsRegex;

        if (!node.Enabled && !validTarget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        }
        else if (!node.Enabled)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        }
        else if (!validTarget)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));
        }

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validTarget)
        {
            ImGui.PopStyleColor();
        }

        if (!validTarget)
        {
            ImGuiEx.TextTooltip("Invalid Target Regex");
        }

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                this.configuration.Save();
                return;
            }

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node);
        this.TextNodeDragDrop(node);
    }

    private void DisplayFolderNode(TextFolderNode node, TextFolderNode root)
    {
        var expanded = ImGui.TreeNodeEx($"{node.Name}##{node.GetHashCode()}-tree");

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (this.configuration.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        this.configuration.Save();
                    }

                    return;
                }

                ImGui.OpenPopup($"{node.GetHashCode()}-popup");
            }
        }

        this.TextNodePopup(node, root);
        this.TextNodeDragDrop(node);

        if (expanded)
        {
            foreach (var childNode in node.Children.ToArray())
            {
                this.DisplayTextNode(childNode, root);
            }

            ImGui.TreePop();
        }
    }

    private void TextNodePopup(INode node, TextFolderNode? root = null)
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
                    this.configuration.Save();
                }

                ImGui.SameLine(100f);
                var isYes = entryNode.IsYes;
                var title = isYes ? "Click Yes" : "Click No";
                if (ImGui.Button(title))
                {
                    entryNode.IsYes = !isYes;
                    this.configuration.Save();
                }

                var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.configuration.Save();
                    }
                }

                var matchText = entryNode.Text;
                if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    entryNode.Text = matchText;
                    this.configuration.Save();
                }

                var zoneRestricted = entryNode.ZoneRestricted;
                if (ImGui.Checkbox("Zone Restricted", ref zoneRestricted))
                {
                    entryNode.ZoneRestricted = zoneRestricted;
                    this.configuration.Save();
                }

                var searchWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.Search);
                var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.Search, "Zone List"))
                {
                    this.zoneListWindow.IsOpen = true;
                }

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - newItemSpacing.X);
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current zone"))
                {
                    var currentId = this.dalamudServices.ClientState.TerritoryType;
                    if (this.module.TerritoryNames.TryGetValue(currentId, out var zoneName))
                    {
                        entryNode.ZoneText = zoneName;
                        this.configuration.Save();
                    }
                    else
                    {
                        entryNode.ZoneText = "Could not find name";
                        this.configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var zoneText = entryNode.ZoneText;
                if (ImGui.InputText($"##{node.Name}-zoneText", ref zoneText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    entryNode.ZoneText = zoneText;
                    this.configuration.Save();
                }
            }

            if (node is ListEntryNode listNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = listNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    listNode.Enabled = enabled;
                    this.configuration.Save();
                }

                var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.configuration.Save();
                    }
                }

                var matchText = listNode.Text;
                if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    listNode.Text = matchText;
                    this.configuration.Save();
                }

                var targetRestricted = listNode.TargetRestricted;
                if (ImGui.Checkbox("Target Restricted", ref targetRestricted))
                {
                    listNode.TargetRestricted = targetRestricted;
                    this.configuration.Save();
                }

                var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                {
                    var target = this.dalamudServices.TargetManager.Target;
                    var name = target?.Name?.TextValue ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        listNode.TargetText = name;
                        this.configuration.Save();
                    }
                    else
                    {
                        listNode.TargetText = "Could not find target";
                        this.configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var targetText = listNode.TargetText;
                if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    listNode.TargetText = targetText;
                    this.configuration.Save();
                }
            }

            if (node is TalkEntryNode talkNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                var enabled = talkNode.Enabled;
                if (ImGui.Checkbox("Enabled", ref enabled))
                {
                    talkNode.Enabled = enabled;
                    this.configuration.Save();
                }

                var trashAltWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.configuration.Save();
                    }
                }

                var searchPlusWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

                ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth - trashAltWidth - newItemSpacing.X);
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Fill with current target"))
                {
                    var target = this.dalamudServices.TargetManager.Target;
                    var name = target?.Name?.TextValue ?? string.Empty;

                    if (!string.IsNullOrEmpty(name))
                    {
                        talkNode.TargetText = name;
                        this.configuration.Save();
                    }
                    else
                    {
                        talkNode.TargetText = "Could not find target";
                        this.configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var targetText = talkNode.TargetText;
                if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    talkNode.TargetText = targetText;
                    this.configuration.Save();
                }
            }

            if (node is TextFolderNode folderNode)
            {
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newItemSpacing);

                if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Add entry"))
                {
                    if (root == this.RootFolder)
                    {
                        var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                    }
                    else if (root == this.ListRootFolder)
                    {
                        var newNode = new ListEntryNode { Enabled = false, Text = "Your text goes here" };
                        folderNode.Children.Add(newNode);
                    }

                    this.configuration.Save();
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.SearchPlus, "Add last seen as new entry"))
                {
                    if (root == this.RootFolder)
                    {
                        var newNode = new TextEntryNode { Enabled = true, Text = this.module.LastSeenDialogText };
                        folderNode.Children.Add(newNode);
                        this.configuration.Save();
                    }
                    else if (root == this.ListRootFolder)
                    {
                        var newNode = new ListEntryNode { Enabled = true, Text = this.module.LastSeenListSelection, TargetRestricted = true, TargetText = this.module.LastSeenListTarget };
                        folderNode.Children.Add(newNode);
                        this.configuration.Save();
                    }
                    else if (root == this.TalkRootFolder)
                    {
                        var newNode = new TalkEntryNode { Enabled = true, TargetText = this.module.LastSeenTalkTarget };
                        folderNode.Children.Add(newNode);
                        this.configuration.Save();
                    }
                }

                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.FolderPlus, "Add folder"))
                {
                    var newNode = new TextFolderNode { Name = "Untitled folder" };
                    folderNode.Children.Add(newNode);
                    this.configuration.Save();
                }

                var trashWidth = ImGuiEx.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);
                ImGui.SameLine(ImGui.GetContentRegionMax().X - trashWidth);
                if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Delete"))
                {
                    if (this.configuration.TryFindParent(node, out var parentNode))
                    {
                        parentNode!.Children.Remove(node);
                        this.configuration.Save();
                    }
                }

                ImGui.PopStyleVar(); // ItemSpacing

                var folderName = folderNode.Name;
                if (ImGui.InputText($"##{node.Name}-rename", ref folderName, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
                {
                    folderNode.Name = folderName;
                    this.configuration.Save();
                }
            }

            ImGui.EndPopup();
        }
    }

    private void TextNodeDragDrop(INode node)
    {
        if (node != this.RootFolder && node != this.ListRootFolder && node != this.TalkRootFolder && ImGui.BeginDragDropSource())
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
                if (this.configuration.TryFindParent(this.draggedNode, out var draggedNodeParent))
                {
                    if (targetNode is TextFolderNode targetFolderNode)
                    {
                        draggedNodeParent!.Children.Remove(this.draggedNode);
                        targetFolderNode.Children.Add(this.draggedNode);
                        this.configuration.Save();
                    }
                    else
                    {
                        if (this.configuration.TryFindParent(targetNode, out var targetNodeParent))
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
                            this.configuration.Save();
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
}