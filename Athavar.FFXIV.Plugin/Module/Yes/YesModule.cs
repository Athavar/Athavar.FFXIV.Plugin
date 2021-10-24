// <copyright file="YesModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Athavar.FFXIV.Plugin;
    using Dalamud.Game;
    using Dalamud.Game.ClientState.Keys;
    using Dalamud.Game.Command;
    using Dalamud.Game.Text.SeStringHandling;
    using Dalamud.Game.Text.SeStringHandling.Payloads;
    using Dalamud.Memory;
    using Dalamud.Utility;

    /// <summary>
    /// Main module implementation.
    /// </summary>
    internal sealed partial class YesModule : IModule
    {
        private const int CurrentConfigVersion = 2;
        private const string Command = "/pyes";

        private static bool setup = false;

        private readonly YesConfigTab configtab;
        private readonly ZoneListWindow zoneListWindow;

        private readonly List<IBaseFeature> features = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="YesModule"/> class.
        /// </summary>
        /// <param name="modules">The other <see cref="Modules"/>.</param>
        public YesModule(Modules modules)
        {
            YesService.Module = this;

            if (!setup)
            {
                YesService.Configuration = modules.Configuration.Yes ??= new();
                YesService.Address = new();
                YesService.Address.Setup();
                setup = true;
            }

            this.LoadTerritories();

            DalamudBinding.Framework.Update += this.FrameworkUpdate;

            this.features.Add(new AddonSelectYesNoFeature());
            this.features.Add(new AddonSelectStringFeature());
            this.features.Add(new AddonSelectIconStringFeature());
            this.features.Add(new AddonSalvageDialogFeature());
            this.features.Add(new AddonMaterializeDialogFeature());
            this.features.Add(new AddonMateriaRetrieveDialogFeature());
            this.features.Add(new AddonItemInspectionResultFeature());
            this.features.Add(new AddonRetainerTaskAskFeature());
            this.features.Add(new AddonRetainerTaskResultFeature());
            this.features.Add(new AddonGrandCompanySupplyRewardFeature());
            this.features.Add(new AddonShopCardDialogFeature());
            this.features.Add(new AddonJournalResultFeature());
            this.features.Add(new AddonContentsFinderConfirmFeature());
            this.features.Add(new AddonTalkFeature());

            this.configtab = new();
            this.zoneListWindow = new();

            Modules.Base.ChangeWindowSystem(ws => ws.AddWindow(this.zoneListWindow));

            DalamudBinding.CommandManager.AddHandler(Command, new CommandInfo(this.OnChatCommand)
            {
                HelpMessage = "Commands that control the yes module.",
                ShowInHelp = true,
            });
        }

        /// <summary>
        /// gets the name of the Module.
        /// </summary>
        public string Name => "Athavar Yes Module";

        /// <summary>
        /// Gets a mapping of territory IDs to names.
        /// </summary>
        internal Dictionary<uint, string> TerritoryNames { get; } = new();

        /// <summary>
        /// Gets or sets the text of the last seen dialog.
        /// </summary>
        internal string LastSeenDialogText { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last selection of a list dialog.
        /// </summary>
        internal string LastSeenListSelection { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target selected when a selection was last made in a list dialog.
        /// </summary>
        internal string LastSeenListTarget { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the target selected when a talk dialog was last updated.
        /// </summary>
        internal string LastSeenTalkTarget { get; set; } = string.Empty;

        /// <summary>
        /// Gets the datetime when the escape button was last pressed.
        /// </summary>
        internal DateTime EscapeLastPressed { get; private set; } = DateTime.MinValue;

        /// <inheritdoc/>
        public void Dispose()
        {
            DalamudBinding.CommandManager.RemoveHandler(Command);
            DalamudBinding.Framework.Update -= this.FrameworkUpdate;

            Modules.Base.ChangeWindowSystem(ws => ws.RemoveWindow(this.zoneListWindow));

            this.features.ForEach(feature => feature?.Dispose());

            YesService.Module = null;
        }

        /// <inheritdoc/>
        public void Draw() => this.configtab.DrawTab();

        #region SeString

        /// <summary>
        /// Read an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal static unsafe SeString GetSeString(byte* textPtr)
            => GetSeString((IntPtr)textPtr);

        /// <summary>
        /// Read an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal static SeString GetSeString(IntPtr textPtr)
            => MemoryHelper.ReadSeStringNullTerminated(textPtr);

        /// <summary>
        /// Read the text of an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal static unsafe string GetSeStringText(byte* textPtr)
            => GetSeStringText(GetSeString(textPtr));

        /// <summary>
        /// Read the text of an SeString.
        /// </summary>
        /// <param name="textPtr">SeString address.</param>
        /// <returns>The SeString.</returns>
        internal static string GetSeStringText(IntPtr textPtr)
            => GetSeStringText(GetSeString(textPtr));

        /// <summary>
        /// Read the text of an SeString.
        /// </summary>
        /// <param name="seString">An SeString.</param>
        /// <returns>The seString.</returns>
        internal static string GetSeStringText(SeString seString)
        {
            var pieces = seString.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
            return text;
        }

        #endregion

        /// <summary>
        /// Print a message to the chat window.
        /// </summary>
        /// <param name="message">Message to display.</param>
        internal void PrintMessage(string message)
        {
            DalamudBinding.ChatGui.Print($"[{this.Name}] {message}");
        }

        /// <summary>
        /// Print a message to the chat window.
        /// </summary>
        /// <param name="message">Message to display.</param>
        internal void PrintMessage(SeString message)
        {
            message.Payloads.Insert(0, new TextPayload($"[{this.Name}] "));
            DalamudBinding.ChatGui.Print(message);
        }

        /// <summary>
        /// Print an error message to the chat window.
        /// </summary>
        /// <param name="message">Message to display.</param>
        internal void PrintError(string message)
        {
            DalamudBinding.ChatGui.PrintError($"[{this.Name}] {message}");
        }

        /// <summary>
        /// Opens the zone list window.
        /// </summary>
        internal void OpenZoneListUi() => this.zoneListWindow.IsOpen = true;

        private void LoadTerritories()
        {
            var sheet = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.TerritoryType>()!;
            foreach (var row in sheet)
            {
                var zone = row.PlaceName.Value;
                if (zone == null)
                {
                    continue;
                }

                var text = GetSeStringText((SeString)zone.Name);
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                this.TerritoryNames.Add(row.RowId, text);
            }
        }

        private void FrameworkUpdate(Framework framework)
        {
            if (DalamudBinding.KeyState[VirtualKey.ESCAPE])
            {
                this.EscapeLastPressed = DateTime.Now;
            }
        }

        #region Commands

        private void OnChatCommand(string command, string arguments)
        {
            if (arguments.IsNullOrEmpty())
            {
                Modules.Base.PluginWindow.Toggle();
                return;
            }

            switch (arguments)
            {
                case "help":
                    this.CommandHelpMenu();
                    break;
                case "toggle":
                    YesService.Configuration.Enabled ^= true;
                    YesService.Configuration.Save();
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
                    this.PrintError("I didn't quite understand that.");
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
            sb.AppendLine($"{Command} lastlist  - Add the last seen target during a Talk dialog.");
            this.PrintMessage(sb.ToString());
        }

        private void CommandAddNode(bool zoneRestricted)
        {
            var text = this.LastSeenDialogText;

            if (text.IsNullOrEmpty())
            {
                if (this.LastSeenDialogText.IsNullOrEmpty())
                {
                    this.PrintError("No dialog has been seen.");
                    return;
                }

                text = this.LastSeenDialogText;
            }

            var newNode = new TextEntryNode { Enabled = true, Text = text };

            if (zoneRestricted)
            {
                var currentID = DalamudBinding.ClientState.TerritoryType;
                if (!this.TerritoryNames.TryGetValue(currentID, out var zoneName))
                {
                    this.PrintError("Could not find zone name.");
                    return;
                }

                newNode.ZoneRestricted = true;
                newNode.ZoneText = zoneName;
            }

            var parent = YesService.Configuration.RootFolder;
            parent.Children.Add(newNode);
            YesService.Configuration.Save();

            this.PrintMessage("Added a new text entry.");
        }

        private void CommandAddListNode()
        {
            var text = this.LastSeenListSelection;
            var target = this.LastSeenListTarget;

            if (text.IsNullOrEmpty())
            {
                this.PrintError("No dialog has been selected.");
                return;
            }

            var newNode = new ListEntryNode { Enabled = true, Text = text };

            if (!target.IsNullOrEmpty())
            {
                newNode.TargetRestricted = true;
                newNode.TargetText = target;
            }

            var parent = YesService.Configuration.ListRootFolder;
            parent.Children.Add(newNode);
            YesService.Configuration.Save();

            this.PrintMessage("Added a new list entry.");
        }

        private void CommandAddTalkNode()
        {
            var target = this.LastSeenTalkTarget;

            if (target.IsNullOrEmpty())
            {
                this.PrintError("No talk dialog has been seen.");
                return;
            }

            var newNode = new TalkEntryNode { Enabled = true, TargetText = target };

            var parent = YesService.Configuration.TalkRootFolder;
            parent.Children.Add(newNode);
            Configuration.Save();

            this.PrintMessage("Added a new talk entry.");
        }
        #endregion
    }
}
