// <copyright file="YesModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Yes;

using System.Text;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Yes.BaseFeatures;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Main module implementation.
/// </summary>
[Module(ModuleName, ModuleConfigurationType = typeof(YesConfiguration), HasTab = true)]
internal sealed class YesModule : Module<YesConfigTab, YesConfiguration>
{
    /// <summary>
    ///     The name of the module.
    /// </summary>
    internal const string ModuleName = "Yes";

    private const int CurrentConfigVersion = 2;
    private const string Command = "/pyes";

    private readonly IServiceProvider provider;
    private readonly IFrameworkManager frameworkManager;
    private readonly List<IBaseFeature> features;
    private ZoneListWindow? zoneListWindow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="YesModule"/> class.
    /// </summary>
    /// <param name="configuration"><see cref="YesConfiguration"/> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager"/> added by DI.</param>
    /// <param name="frameworkManager"><see cref="IFrameworkManager"/> added by DI.</param>
    /// <param name="addressResolver"><see cref="AddressResolver"/> added by DI.</param>
    public YesModule(YesConfiguration configuration, IServiceProvider provider, IDalamudServices dalamudServices, IChatManager chatManager, IFrameworkManager frameworkManager, AddressResolver addressResolver)
        : base(configuration)
    {
        this.provider = provider;
        this.DalamudServices = dalamudServices;
        this.ChatManager = chatManager;
        this.Logger = dalamudServices.PluginLogger;
        this.frameworkManager = frameworkManager;
        this.AddressResolver = addressResolver;

        this.LoadTerritories();

        this.frameworkManager.Subscribe(this.FrameworkUpdate);

        this.features = new List<IBaseFeature>();
        foreach (var baseFeatureTypes in this.GetType()
           .Assembly.GetTypes()
           .Where(t => t is { IsAbstract: false, IsInterface: false } && t.IsAssignableTo(typeof(IBaseFeature))))
        {
            this.Logger.Verbose("[Yes] Create IBaseFeature {0}", baseFeatureTypes.FullName!);
            this.features.Add((IBaseFeature)Activator.CreateInstance(baseFeatureTypes, this)!);
        }

        this.DalamudServices.CommandManager.AddHandler(Command, new CommandInfo(this.OnChatCommand)
        {
            HelpMessage = "Commands that control the yes module.",
            ShowInHelp = true,
        });

        dalamudServices.PluginLogger.Debug("Module 'Yes' init");
    }

    /// <inheritdoc/>
    public override string Name => ModuleName;

    internal YesConfiguration MC => this.ModuleConfig;

    /// <summary>
    ///     Gets the <see cref="IDalamudServices"/>.
    /// </summary>
    internal IDalamudServices DalamudServices { get; }

    /// <summary>
    ///     Gets the <see cref="IChatManager"/>.
    /// </summary>
    internal IChatManager ChatManager { get; }

    /// <summary>
    ///     Gets the <see cref="IPluginLogger"/>.
    /// </summary>
    internal IPluginLogger Logger { get; }

    /// <summary>
    ///     Gets the <see cref="AddressResolver"/>.
    /// </summary>
    internal AddressResolver AddressResolver { get; }

    /// <summary>
    ///     Gets a mapping of territory IDs to names.
    /// </summary>
    internal Dictionary<uint, string> TerritoryNames { get; } = new();

    /// <summary>
    ///     Gets the target name when the escape button was last pressed.
    /// </summary>
    internal string EscapeTargetName { get; } = string.Empty;

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

    /// <summary>
    ///     Gets or sets the last selected list node, so the escape only skips that specific one.
    /// </summary>
    internal ListEntryNode LastSelectedListNode { get; set; } = new();

    /// <inheritdoc/>
    public override void Dispose()
    {
        this.DalamudServices.CommandManager.RemoveHandler(Command);
        this.frameworkManager.Unsubscribe(this.FrameworkUpdate);
        base.Dispose();

        this.features.ForEach(feature => feature?.Dispose());
        this.zoneListWindow?.Dispose();
    }

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
    internal unsafe SeString GetSeString(byte* textPtr) => this.GetSeString((nint)textPtr);

    /// <summary>
    ///     Read an SeString.
    /// </summary>
    /// <param name="textPtr">SeString address.</param>
    /// <returns>The SeString.</returns>
    internal SeString GetSeString(nint textPtr) => MemoryHelper.ReadSeStringNullTerminated(textPtr);

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
    internal string GetSeStringText(nint textPtr) => this.GetSeStringText(this.GetSeString(textPtr));

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

    protected override YesConfigTab InitTab()
    {
        this.zoneListWindow = ActivatorUtilities.CreateInstance<ZoneListWindow>(this.provider, this);
        return ActivatorUtilities.CreateInstance<YesConfigTab>(this.provider, this, this.zoneListWindow);
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

    private void FrameworkUpdate(IFramework framework1)
    {
        this.DisableKeyPressed = this.ModuleConfig.DisableKey != (int)VirtualKey.NO_KEY && this.DalamudServices.KeyState[this.ModuleConfig.DisableKey];

        this.ForcedYesKeyPressed = this.ModuleConfig.ForcedYesKey != (int)VirtualKey.NO_KEY && this.DalamudServices.KeyState[this.ModuleConfig.ForcedYesKey];

        if (this.DalamudServices.KeyState[VirtualKey.ESCAPE])
        {
            this.EscapeLastPressed = DateTime.Now;
        }
    }

    private void OnChatCommand(string command, string arguments)
    {
        switch (arguments.Trim())
        {
            case "":
            case "help":
                this.CommandHelpMenu();
                break;
            case "toggle":
                this.ModuleConfig.FunctionEnabled ^= true;
                this.ModuleConfig.Save();
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

        this.CreateTextNode(this.ModuleConfig.RootFolder, zoneRestricted, createFolder, selectNo);
        this.ModuleConfig.Save();

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

        var parent = this.ModuleConfig.ListRootFolder;
        parent.Children.Add(newNode);
        this.ModuleConfig.Save();

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

        var parent = this.ModuleConfig.TalkRootFolder;
        parent.Children.Add(newNode);
        this.ModuleConfig.Save();

        this.ChatManager.PrintChat("Added a new talk entry.");
    }

    private void ToggleDutyConfirm()
    {
        this.ModuleConfig.ContentsFinderConfirmEnabled ^= true;
        this.ModuleConfig.ContentsFinderOneTimeConfirmEnabled = false;
        this.ModuleConfig.Save();

        var state = this.ModuleConfig.ContentsFinderConfirmEnabled ? "enabled" : "disabled";
        this.ChatManager.PrintChat($"Duty Confirm {state}.");
    }

    private void ToggleOneTimeConfirm()
    {
        this.ModuleConfig.ContentsFinderOneTimeConfirmEnabled ^= true;
        this.ModuleConfig.ContentsFinderConfirmEnabled = this.ModuleConfig.ContentsFinderOneTimeConfirmEnabled;
        this.ModuleConfig.Save();

        var state = this.ModuleConfig.ContentsFinderOneTimeConfirmEnabled ? "enabled" : "disabled";
        this.ChatManager.PrintChat($"Duty Confirm and One Time Confirm {state}.");
    }
}