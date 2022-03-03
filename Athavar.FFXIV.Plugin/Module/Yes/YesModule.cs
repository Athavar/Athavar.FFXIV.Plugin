// <copyright file="YesModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Athavar.FFXIV.Plugin.Manager;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Yes.BaseFeatures;
using Athavar.FFXIV.Plugin.Module.Yes.Features;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;

/// <summary>
///     Main module implementation.
/// </summary>
internal sealed class YesModule : IModule, IDisposable
{
    /// <summary>
    ///     The name of the module.
    /// </summary>
    internal const string ModuleName = "Yes";

    private const int CurrentConfigVersion = 2;
    private const string Command = "/pyes";
    private readonly PluginWindow pluginWindow;

    private readonly YesConfigTab configTab;

    private readonly List<IBaseFeature> features = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="YesModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="ModuleManager" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="addressResolver"><see cref="PluginAddressResolver" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    /// <param name="configTab"><see cref="YesConfigTab" /> added by DI.</param>
    /// <param name="pluginWindow"><see cref="PluginWindow" /> added by DI.</param>
    public YesModule(IModuleManager moduleManager, IDalamudServices dalamudServices, PluginAddressResolver addressResolver, Configuration configuration, IChatManager chatManager, YesConfigTab configTab, PluginWindow pluginWindow)
    {
        this.DalamudServices = dalamudServices;
        this.ChatManager = chatManager;
        this.configTab = configTab;
        this.pluginWindow = pluginWindow;
        this.AddressResolver = addressResolver;

        this.Configuration = configuration.Yes ??= new YesConfiguration();

        this.LoadTerritories();

        this.DalamudServices.Framework.Update += this.FrameworkUpdate;

        this.features.Add(new AddonSelectYesNoFeature(this));
        this.features.Add(new AddonSelectStringFeature(this));
        this.features.Add(new AddonSelectIconStringFeature(this));
        this.features.Add(new AddonSalvageDialogFeature(this));
        this.features.Add(new AddonMaterializeDialogFeature(this));
        this.features.Add(new AddonMateriaRetrieveDialogFeature(this));
        this.features.Add(new AddonItemInspectionResultFeature(this));
        this.features.Add(new AddonRetainerTaskAskFeature(this));
        this.features.Add(new AddonRetainerTaskResultFeature(this));
        this.features.Add(new AddonGrandCompanySupplyRewardFeature(this));
        this.features.Add(new AddonShopCardDialogFeature(this));
        this.features.Add(new AddonJournalResultFeature(this));
        this.features.Add(new AddonContentsFinderConfirmFeature(this));
        this.features.Add(new AddonTalkFeature(this));

        this.DalamudServices.CommandManager.AddHandler(Command, new CommandInfo(this.OnChatCommand)
                                                                {
                                                                    HelpMessage = "Commands that control the yes module.",
                                                                    ShowInHelp = true,
                                                                });

        this.configTab.Setup(this);
        moduleManager.Register(this, this.Configuration.ModuleEnabled);
    }

    /// <summary>
    ///     gets the name of the Module.
    /// </summary>
    public string Name => ModuleName;

    /// <summary>
    ///     Gets the <see cref="IDalamudServices" />.
    /// </summary>
    internal IDalamudServices DalamudServices { get; }

    /// <summary>
    ///     Gets the yes module configuration.
    /// </summary>
    internal YesConfiguration Configuration { get; }

    /// <summary>
    ///     Gets the <see cref="IChatManager" />.
    /// </summary>
    internal IChatManager ChatManager { get; }

    /// <summary>
    ///     Gets the <see cref="PluginAddressResolver" />.
    /// </summary>
    internal PluginAddressResolver AddressResolver { get; }

    /// <summary>
    ///     Gets a mapping of territory IDs to names.
    /// </summary>
    internal Dictionary<uint, string> TerritoryNames { get; } = new();

    /// <summary>
    ///     Gets or sets the text of the last seen dialog.
    /// </summary>
    internal string LastSeenDialogText { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the last selection of a list dialog.
    /// </summary>
    internal string LastSeenListSelection { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the target selected when a selection was last made in a list dialog.
    /// </summary>
    internal string LastSeenListTarget { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the target selected when a talk dialog was last updated.
    /// </summary>
    internal string LastSeenTalkTarget { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the datetime when the escape button was last pressed.
    /// </summary>
    internal DateTime EscapeLastPressed { get; private set; } = DateTime.MinValue;

    /// <summary>
    ///     Gets a value indicating whether the disable hotkey is pressed.
    /// </summary>
    internal bool DisableKeyPressed { get; private set; }

    /// <summary>
    ///     Gets a value indicating whether the forced yes hotkey is pressed.
    /// </summary>
    internal bool ForcedYesKeyPressed { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        this.DalamudServices.CommandManager.RemoveHandler(Command);
        this.DalamudServices.Framework.Update -= this.FrameworkUpdate;

        this.features.ForEach(feature => feature?.Dispose());
    }

    /// <inheritdoc />
    public void Draw() => this.configTab.DrawTab();

    public void Enable(bool state = true) => this.Configuration.ModuleEnabled = state;

    /// <summary>
    ///     Create a new node with various options.
    /// </summary>
    /// <param name="folder">Folder to place the node in.</param>
    /// <param name="zoneRestricted">Create the node restricted to the current zone.</param>
    /// <param name="createFolder">Create a zone named subfolder.</param>
    /// <param name="selectNo">Select no instead.</param>
    public void CreateTextNode(TextFolderNode folder, bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var newNode = new TextEntryNode { Enabled = true, Text = this.LastSeenDialogText };
        var chosenFolder = folder;

        if (zoneRestricted || createFolder)
        {
            var currentId = this.DalamudServices.ClientState.TerritoryType;
            if (!this.TerritoryNames.TryGetValue(currentId, out var zoneName))
            {
                return;
            }

            newNode.ZoneRestricted = true;
            newNode.ZoneText = zoneName;
        }

        if (createFolder)
        {
            var zoneName = newNode.ZoneText;

            chosenFolder = folder.Children.OfType<TextFolderNode>().FirstOrDefault(node => node.Name == zoneName);
            if (chosenFolder == default)
            {
                chosenFolder = new TextFolderNode { Name = zoneName };
                folder.Children.Add(chosenFolder);
            }
        }

        if (selectNo)
        {
            newNode.IsYes = false;
        }

        chosenFolder.Children.Add(newNode);
    }

    /// <summary>
    ///     Read an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal unsafe SeString GetSeString(byte* textPtr) => this.GetSeString((IntPtr)textPtr);

    /// <summary>
    ///     Read an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal SeString GetSeString(IntPtr textPtr) => MemoryHelper.ReadSeStringNullTerminated(textPtr);

    /// <summary>
    ///     Read the text of an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal unsafe string GetSeStringText(byte* textPtr) => this.GetSeStringText(this.GetSeString(textPtr));

    /// <summary>
    ///     Read the text of an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal string GetSeStringText(IntPtr textPtr) => this.GetSeStringText(this.GetSeString(textPtr));

    /// <summary>
    ///     Read the text of an SeString.
    /// </summary>
    /// <param name="seString">An SeString.</param>
    /// <returns>The seString.</returns>
    internal string GetSeStringText(SeString seString)
    {
        var pieces = seString.Payloads.OfType<TextPayload>().Select(t => t.Text);
        var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
        return text;
    }

    private void LoadTerritories()
    {
        var sheet = this.DalamudServices.DataManager.GetExcelSheet<TerritoryType>()!;
        foreach (var row in sheet)
        {
            var zone = row.PlaceName.Value;
            if (zone == null)
            {
                continue;
            }

            var text = this.GetSeStringText((SeString)zone.Name);
            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            this.TerritoryNames.Add(row.RowId, text);
        }
    }

    private void FrameworkUpdate(Framework framework)
    {
        this.DisableKeyPressed = this.Configuration.DisableKey != (int)VirtualKey.NO_KEY && this.DalamudServices.KeyState[this.Configuration.DisableKey];

        this.ForcedYesKeyPressed = this.Configuration.ForcedYesKey != (int)VirtualKey.NO_KEY && this.DalamudServices.KeyState[this.Configuration.ForcedYesKey];

        if (this.DalamudServices.KeyState[VirtualKey.ESCAPE])
        {
            this.EscapeLastPressed = DateTime.Now;
        }
    }

    private void OnChatCommand(string command, string arguments)
    {
        if (arguments.IsNullOrEmpty())
        {
            this.pluginWindow.Toggle();
            return;
        }

        switch (arguments)
        {
            case "help":
                this.CommandHelpMenu();
                break;
            case "toggle":
                this.Configuration.FunctionEnabled ^= true;
                this.Configuration.Save();
                break;
            case "last":
                this.CommandAddNode(false, false, false);
                break;
            case "last no":
                this.CommandAddNode(false, false, true);
                break;
            case "last zone":
                this.CommandAddNode(true, false, false);
                break;
            case "last zone no":
                this.CommandAddNode(true, false, true);
                break;
            case "last zone folder":
                this.CommandAddNode(true, true, false);
                break;
            case "last zone folder no":
                this.CommandAddNode(true, true, true);
                break;
            case "lastlist":
                this.CommandAddListNode();
                break;
            case "lasttalk":
                this.CommandAddTalkNode();
                break;
            case "dutyconfirm":
                this.ToggleDutyConfirm();
                break;
            case "onetimeconfirm":
                this.ToggleOneTimeConfirm();
                break;
            default:
                this.ChatManager.PrintErrorMessage("I didn't quite understand that.");
                return;
        }
    }

    private void CommandHelpMenu()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Help menu");
        sb.AppendLine($"{Command} - Toggle the config window.");
        sb.AppendLine($"{Command} toggle - Toggle the plugin on/off.");
        sb.AppendLine($"{Command} last - Add the last seen YesNo dialog.");
        sb.AppendLine($"{Command} last no - Add the last seen YesNo dialog as a no.");
        sb.AppendLine($"{Command} last zone - Add the last seen YesNo dialog with the current zone name.");
        sb.AppendLine($"{Command} last zone no - Add the last seen YesNo dialog with the current zone name as a no.");
        sb.AppendLine($"{Command} last zone folder - Add the last seen YesNo dialog with the current zone name in a folder with the current zone name.");
        sb.AppendLine($"{Command} last zone folder no - Add the last seen YesNo dialog with the current zone name in a folder with the current zone name as a no.");
        sb.AppendLine($"{Command} lastlist - Add the last selected list dialog with the target at the time.");
        sb.AppendLine($"{Command} lasttalk - Add the last seen target during a Talk dialog.");
        sb.AppendLine($"{Command} dutyconfirm - Toggle duty confirm.");
        sb.AppendLine($"{Command} onetimeconfirm - Toggles duty confirm as well as one-time confirm.");
        this.ChatManager.PrintChat(sb.ToString());
    }

    private void CommandAddNode(bool zoneRestricted, bool createFolder, bool selectNo)
    {
        var text = this.LastSeenDialogText;

        if (text.IsNullOrEmpty())
        {
            this.ChatManager.PrintErrorMessage("No dialog has been seen.");
            return;
        }

        this.CreateTextNode(this.Configuration.RootFolder, zoneRestricted, createFolder, selectNo);
        this.Configuration.Save();

        this.ChatManager.PrintChat("Added a new text entry.");
    }

    private void CommandAddListNode()
    {
        var text = this.LastSeenListSelection;
        var target = this.LastSeenListTarget;

        if (text.IsNullOrEmpty())
        {
            this.ChatManager.PrintErrorMessage("No dialog has been selected.");
            return;
        }

        var newNode = new ListEntryNode { Enabled = true, Text = text };

        if (!target.IsNullOrEmpty())
        {
            newNode.TargetRestricted = true;
            newNode.TargetText = target;
        }

        var parent = this.Configuration.ListRootFolder;
        parent.Children.Add(newNode);
        this.Configuration.Save();

        this.ChatManager.PrintChat("Added a new list entry.");
    }

    private void CommandAddTalkNode()
    {
        var target = this.LastSeenTalkTarget;

        if (target.IsNullOrEmpty())
        {
            this.ChatManager.PrintErrorMessage("No talk dialog has been seen.");
            return;
        }

        var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

        var parent = this.Configuration.TalkRootFolder;
        parent.Children.Add(newNode);
        this.Configuration.Save();

        this.ChatManager.PrintChat("Added a new talk entry.");
    }

    private void ToggleDutyConfirm()
    {
        this.Configuration.ContentsFinderConfirmEnabled ^= true;
        this.Configuration.ContentsFinderOneTimeConfirmEnabled = false;
        this.Configuration.Save();

        var state = this.Configuration.ContentsFinderConfirmEnabled ? "enabled" : "disabled";
        this.ChatManager.PrintChat($"Duty Confirm {state}.");
    }

    private void ToggleOneTimeConfirm()
    {
        this.Configuration.ContentsFinderOneTimeConfirmEnabled ^= true;
        this.Configuration.ContentsFinderConfirmEnabled = this.Configuration.ContentsFinderOneTimeConfirmEnabled;
        this.Configuration.Save();

        var state = this.Configuration.ContentsFinderOneTimeConfirmEnabled ? "enabled" : "disabled";
        this.ChatManager.PrintChat($"Duty Confirm and One Time Confirm {state}.");
    }
}