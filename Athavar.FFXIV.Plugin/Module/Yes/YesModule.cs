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
using ImGuiNET;
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
    ///     Read an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal unsafe SeString GetSeString(byte* textPtr)
        =>
            this.GetSeString((IntPtr)textPtr);

    /// <summary>
    ///     Read an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal SeString GetSeString(IntPtr textPtr)
        => MemoryHelper.ReadSeStringNullTerminated(textPtr);

    /// <summary>
    ///     Read the text of an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal unsafe string GetSeStringText(byte* textPtr)
        =>
            this.GetSeStringText(this.GetSeString(textPtr));

    /// <summary>
    ///     Read the text of an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal string GetSeStringText(IntPtr textPtr)
        =>
            this.GetSeStringText(this.GetSeString(textPtr));

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
                this.Configuration.Enabled ^= true;
                this.Configuration.Save();
                break;
            case "last":
                this.CommandAddNode(false);
                break;
            case "last zone":
                this.CommandAddNode(true);
                break;
            case "lastlist":
                this.CommandAddListNode();
                break;
            case "lasttalk":
                this.CommandAddTalkNode();
                break;
            default:
                this.ChatManager.PrintError("I didn't quite understand that.");
                return;
        }
    }

    private void CommandHelpMenu()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Help menu");
        sb.AppendLine($"{Command}           - Toggle the config window.");
        sb.AppendLine($"{Command} toggle    - Toggle the plugin on/off.");
        sb.AppendLine($"{Command} last      - Add the last seen YesNo dialog.");
        sb.AppendLine($"{Command} last zone - Add the last seen YesNo dialog with the current zone name.");
        sb.AppendLine($"{Command} lastlist  - Add the last selected list dialog with the target at the time.");
        sb.AppendLine($"{Command} lasttalk  - Add the last seen target during a Talk dialog.");
        this.ChatManager.PrintMessage(sb.ToString());
    }

    private void CommandAddNode(bool zoneRestricted)
    {
        var text = this.LastSeenDialogText;

        if (text.IsNullOrEmpty())
        {
            if (this.LastSeenDialogText.IsNullOrEmpty())
            {
                this.ChatManager.PrintError("No dialog has been seen.");
                return;
            }

            text = this.LastSeenDialogText;
        }

        var newNode = new TextEntryNode { Enabled = true, Text = text };

        if (zoneRestricted)
        {
            var currentId = this.DalamudServices.ClientState.TerritoryType;
            if (!this.TerritoryNames.TryGetValue(currentId, out var zoneName))
            {
                this.ChatManager.PrintError("Could not find zone name.");
                return;
            }

            newNode.ZoneRestricted = true;
            newNode.ZoneText = zoneName;
        }

        var parent = this.Configuration.RootFolder;
        parent.Children.Add(newNode);
        this.Configuration.Save();

        this.ChatManager.PrintMessage("Added a new text entry.");
    }

    private void CommandAddListNode()
    {
        var text = this.LastSeenListSelection;
        var target = this.LastSeenListTarget;

        if (text.IsNullOrEmpty())
        {
            this.ChatManager.PrintError("No dialog has been selected.");
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

        this.ChatManager.PrintMessage("Added a new list entry.");
    }

    private void CommandAddTalkNode()
    {
        var target = this.LastSeenTalkTarget;

        if (target.IsNullOrEmpty())
        {
            this.ChatManager.PrintError("No talk dialog has been seen.");
            return;
        }

        var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

        var parent = this.Configuration.TalkRootFolder;
        parent.Children.Add(newNode);
        this.Configuration.Save();

        this.ChatManager.PrintMessage("Added a new talk entry.");
    }
}