namespace Athavar.FFXIV.Plugin.Module.Yes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using Athavar.FFXIV.Plugin;
    using ClickLib;
    using Dalamud.Game.Command;
    using Dalamud.Game.Text.SeStringHandling;
    using Dalamud.Game.Text.SeStringHandling.Payloads;
    using Dalamud.Hooking;
    using Dalamud.Logging;
    using Dalamud.Memory;
    using Dalamud.Utility;
    using FFXIVClientStructs.FFXIV.Client.UI;
    using FFXIVClientStructs.FFXIV.Component.GUI;
    using Lumina.Excel.GeneratedSheets;

    /// <summary>
    /// Main module implementation.
    /// </summary>
    internal sealed partial class YesModule : IModule
    {
        private const int CurrentConfigVersion = 2;
        private const string Command = "/pyes";

        private readonly YesConfigTab configtab;
        private readonly ZoneListWindow zoneListWindow;

        private readonly List<Hook<OnSetupDelegate>> onSetupHooks = new();
        private readonly Hook<OnSetupDelegate> addonSelectYesNoOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonSalvageDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonMaterializeDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonMateriaRetrieveDialogOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonItemInspectionResultOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonRetainerTaskAskOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonRetainerTaskResultOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonGrandCompanySupplyRewardOnSetupHook;
        private readonly Hook<OnSetupDelegate> addonShopCardDialogOnSetupHook;

        /// <summary>
        /// Initializes a new instance of the <see cref="YesModule"/> class.
        /// </summary>
        /// <param name="modules">The other <see cref="Modules"/>.</param>
        public YesModule(Modules modules)
        {
            this.Configuration = modules.Configuration.Yes ??= new();

            this.Address = new YesAddressResolver();
            this.Address.Setup();

            this.LoadTerritories();

            this.onSetupHooks.Add(this.addonSelectYesNoOnSetupHook = new(this.Address.AddonSelectYesNoOnSetupAddress, this.AddonSelectYesNoOnSetupDetour));
            this.onSetupHooks.Add(this.addonSalvageDialogOnSetupHook = new(this.Address.AddonSalvageDialongOnSetupAddress, this.AddonSalvageDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonMaterializeDialogOnSetupHook = new(this.Address.AddonMaterializeDialongOnSetupAddress, this.AddonMaterializeDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonMateriaRetrieveDialogOnSetupHook = new(this.Address.AddonMateriaRetrieveDialongOnSetupAddress, this.AddonMateriaRetrieveDialogOnSetupDetour));
            this.onSetupHooks.Add(this.addonItemInspectionResultOnSetupHook = new(this.Address.AddonItemInspectionResultOnSetupAddress, this.AddonItemInspectionResultOnSetupDetour));
            this.onSetupHooks.Add(this.addonRetainerTaskAskOnSetupHook = new(this.Address.AddonRetainerTaskAskOnSetupAddress, this.AddonRetainerTaskAskOnSetupDetour));
            this.onSetupHooks.Add(this.addonRetainerTaskResultOnSetupHook = new(this.Address.AddonRetainerTaskResultOnSetupAddress, this.AddonRetainerTaskResultOnSetupDetour));
            this.onSetupHooks.Add(this.addonGrandCompanySupplyRewardOnSetupHook = new(this.Address.AddonGrandCompanySupplyRewardOnSetupAddress, this.AddonGrandCompanySupplyRewardOnSetupDetour));
            this.onSetupHooks.Add(this.addonShopCardDialogOnSetupHook = new(this.Address.AddonShopCardDialogOnSetupAddress, this.AddonShopCardDialogOnSetupDetour));
            this.onSetupHooks.ForEach(hook => hook.Enable());

            this.configtab = new(this);
            this.zoneListWindow = new(this);

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
        /// Gets the configuration of the <see cref="YesModule"/>.
        /// </summary>
        internal YesConfiguration Configuration { get; private set; }

        /// <summary>
        /// Gets the <see cref="YesAddressResolver"/> of the <see cref="YesModule"/>.
        /// </summary>
        internal YesAddressResolver Address { get; private set; }

        /// <summary>
        /// Gets a mapping of territory IDs to names.
        /// </summary>
        internal Dictionary<uint, string> TerritoryNames { get; } = new();

        /// <summary>
        /// Gets or sets the text of the last seen dialog.
        /// </summary>
        internal string LastSeenDialogText { get; set; } = string.Empty;

        /// <inheritdoc/>
        public void Dispose()
        {
            DalamudBinding.CommandManager.RemoveHandler(Command);

            Modules.Base.ChangeWindowSystem(ws => ws.RemoveWindow(this.zoneListWindow));

            this.onSetupHooks.ForEach(hook => hook.Dispose());
        }

        /// <inheritdoc/>
        public void Draw() => this.configtab.DrawTab();

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

                var text = this.GetSeStringText((SeString)zone.Name);
                if (string.IsNullOrEmpty(text))
                {
                    continue;
                }

                this.TerritoryNames.Add(row.RowId, text);
            }
        }

        #region SeString

        private unsafe SeString GetSeString(byte* textPtr)
        {
            return this.GetSeString((IntPtr)textPtr);
        }

        private SeString GetSeString(IntPtr textPtr)
        {
            return MemoryHelper.ReadSeStringNullTerminated(textPtr);
        }

        private SeString GetSeString(byte[] bytes)
        {
            return SeString.Parse(bytes);
        }

        private string GetSeStringText(SeString sestring)
        {
            var pieces = sestring.Payloads.OfType<TextPayload>().Select(t => t.Text);
            var text = string.Join(string.Empty, pieces).Replace('\n', ' ').Trim();
            return text;
        }

        #endregion

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
                case "last":
                    this.CommandAddNode(this.LastSeenDialogText, false, this.Configuration.RootFolder);
                    break;
                case "last zone":
                    this.CommandAddNode(this.LastSeenDialogText, true, this.Configuration.RootFolder);
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
            sb.AppendLine($"{Command} last      - Add the last seen YesNo dialog.");
            sb.AppendLine($"{Command} last zone - Add the last seen YesNo dialog with the current zone name.");
            this.PrintMessage(sb.ToString());
        }

        private void CommandAddNode(string text, bool zoneRestricted, TextFolderNode parent)
        {
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

            parent.Children.Add(newNode);
            this.Configuration.Save();

            this.PrintMessage("Added a new text entry.");
        }
        #endregion
    }

    /// <summary>
    /// YesNo text matching features.
    /// </summary>
    internal sealed partial class YesModule
    {
        private bool EntryMatchesText(TextEntryNode node, string text)
        {
            return (node.IsTextRegex && (node.TextRegex?.IsMatch(text) ?? false)) ||
                  (!node.IsTextRegex && text.Contains(node.Text));
        }

        private bool EntryMatchesZoneName(TextEntryNode node, string zoneName)
        {
            return (node.ZoneIsRegex && (node.ZoneRegex?.IsMatch(zoneName) ?? false)) ||
                  (!node.ZoneIsRegex && zoneName.Contains(node.ZoneText));
        }

        private void AddonSelectYesNoExecute(IntPtr addon)
        {
            unsafe
            {
                var addonObj = (AddonSelectYesno*)addon;
                var yesButton = addonObj->YesButton;
                if (yesButton != null && !yesButton->IsEnabled)
                {
                    PluginLog.Debug($"AddonSelectYesNo: Enabling yes button");
                    yesButton->AtkComponentBase.OwnerNode->AtkResNode.Flags ^= 1 << 5;
                }
            }

            PluginLog.Debug($"AddonSelectYesNo: Selecting yes");
            Click.SendClick("select_yes", addon);
        }

        private IntPtr AddonSelectYesNoOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSelectYesNo.OnSetup");
            var result = this.addonSelectYesNoOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                var data = Marshal.PtrToStructure<AddonSelectYesNoOnSetupData>(dataPtr);
                var text = this.LastSeenDialogText = this.GetSeStringText(this.GetSeString(data.TextPtr));

                PluginLog.Debug($"AddonSelectYesNo: text={text}");

                if (this.Configuration.Enabled)
                {
                    var nodes = this.Configuration.GetAllNodes().OfType<TextEntryNode>();
                    var zoneWarnOnce = true;
                    foreach (var node in nodes)
                    {
                        if (node.Enabled && !string.IsNullOrEmpty(node.Text) && this.EntryMatchesText(node, text))
                        {
                            if (node.ZoneRestricted && !string.IsNullOrEmpty(node.ZoneText))
                            {
                                if (!this.TerritoryNames.TryGetValue(DalamudBinding.ClientState.TerritoryType, out var zoneName))
                                {
                                    if (zoneWarnOnce && !(zoneWarnOnce = false))
                                    {
                                        PluginLog.Debug("Unable to verify Zone Restricted entry, ZoneID was not set yet");
                                        this.PrintMessage($"Unable to verify Zone Restricted entry, change zones to update value");
                                    }

                                    zoneName = string.Empty;
                                }

                                if (!string.IsNullOrEmpty(zoneName) && this.EntryMatchesZoneName(node, zoneName))
                                {
                                    PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text} ({node.ZoneText})");
                                    this.AddonSelectYesNoExecute(addon);
                                    break;
                                }
                            }
                            else
                            {
                                PluginLog.Debug($"AddonSelectYesNo: Matched on {node.Text}");
                                this.AddonSelectYesNoExecute(addon);
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        [StructLayout(LayoutKind.Explicit, Size = 0x10)]
        private struct AddonSelectYesNoOnSetupData
        {
            [FieldOffset(0x8)]
            public IntPtr TextPtr;
        }
    }

    /// <summary>
    /// Non text matching features.
    /// </summary>
    internal sealed partial class YesModule
    {
        private void SendClicks(bool enabled, params string[] clicks)
        {
            try
            {
                if (this.Configuration.Enabled && enabled)
                {
                    foreach (var click in clicks)
                    {
                        Click.SendClick(click);
                    }
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }
        }

        private IntPtr AddonSalvageDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonSalvageDialog.OnSetup");

            var result = this.addonSalvageDialogOnSetupHook.Original(addon, a2, dataPtr);

            try
            {
                if (this.Configuration.Enabled && this.Configuration.DesynthBulkDialogEnabled)
                {
                    unsafe
                    {
                        ((AddonSalvageDialog*)addon)->BulkDesynthEnabled = true;
                    }
                }

                if (this.Configuration.Enabled && this.Configuration.DesynthDialogEnabled)
                {
                    Click.SendClick("desynthesize_checkbox");
                    Click.SendClick("desynthesize");
                }
            }
            catch (Exception ex)
            {
                PluginLog.Error(ex, "Don't crash the game");
            }

            return result;
        }

        private IntPtr AddonMaterializeDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonMaterializeDialog.OnSetupDetour");
            var result = this.addonMaterializeDialogOnSetupHook.Original(addon, a2, dataPtr);
            this.SendClicks(this.Configuration.MaterializeDialogEnabled, "materialize");
            return result;
        }

        private IntPtr AddonMateriaRetrieveDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug("AddonMateriaRetrieveDialog.OnSetupDetour");
            var result = this.addonMateriaRetrieveDialogOnSetupHook.Original(addon, a2, dataPtr);
            this.SendClicks(this.Configuration.MateriaRetrieveDialogEnabled, "retrieve_materia");
            return result;
        }

        private int itemInspectionCount = 0;
        private int itemInspectionLimit = 10;

        private IntPtr AddonItemInspectionResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonItemInspectionResult.OnSetup");

            var result = this.addonItemInspectionResultOnSetupHook.Original(addon, a2, dataPtr);

            if (this.Configuration.ItemInspectionResultEnabled)
            {
                unsafe
                {
                    var addonPtr = (AddonItemInspectionResult*)addon;

                    if (addonPtr->AtkUnitBase.UldManager.NodeListCount >= 64)
                    {
                        var nameNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[64];
                        var descNode = (AtkTextNode*)addonPtr->AtkUnitBase.UldManager.NodeList[55];
                        if (nameNode->AtkResNode.IsVisible && descNode->AtkResNode.IsVisible)
                        {
                            var nameText = this.GetSeString(nameNode->NodeText.StringPtr);
                            var descText = this.GetSeStringText(this.GetSeString(descNode->NodeText.StringPtr));
                            // This is hackish, but works well enough (for now)
                            // French doesn't have the widget
                            if (descText.Contains("※") || descText.Contains("liées à Garde-la-Reine"))
                            {
                                nameText.Payloads.Insert(0, new TextPayload("Received: "));
                                this.PrintMessage(nameText);
                            }
                        }
                    }
                }
            }

            this.itemInspectionCount++;
            if (this.itemInspectionCount % this.itemInspectionLimit == 0)
            {
                this.PrintMessage("Sanity check, pausing item inspection loop.");
            }
            else
            {
                this.SendClicks(this.Configuration.ItemInspectionResultEnabled, "item_inspection_result_next");
            }

            return result;
        }

        private IntPtr AddonRetainerTaskAskOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerTaskAsk.OnSetup");
            var result = this.addonRetainerTaskAskOnSetupHook.Original(addon, a2, dataPtr);
            this.SendClicks(this.Configuration.RetainerTaskAskEnabled, "retainer_venture_ask_assign");
            return result;
        }

        private IntPtr AddonRetainerTaskResultOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonRetainerTaskResult.OnSetup");
            var result = this.addonRetainerTaskResultOnSetupHook.Original(addon, a2, dataPtr);
            this.SendClicks(this.Configuration.RetainerTaskResultEnabled, "retainer_venture_result_reassign", "retainer_venture_result_reassign");
            return result;
        }

        private IntPtr AddonGrandCompanySupplyRewardOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonGrandCompanySupplyReward.OnSetup");
            var result = this.addonGrandCompanySupplyRewardOnSetupHook.Original(addon, a2, dataPtr);
            this.SendClicks(this.Configuration.GrandCompanySupplyReward, "grand_company_expert_delivery_deliver");
            return result;
        }

        private IntPtr AddonShopCardDialogOnSetupDetour(IntPtr addon, uint a2, IntPtr dataPtr)
        {
            PluginLog.Debug($"AddonShopCardDialog.OnSetup");
            var result = this.addonShopCardDialogOnSetupHook.Original(addon, a2, dataPtr);
            this.SendClicks(this.Configuration.ShopCardDialog, "sell_triple_triad_card");
            return result;
        }
    }
}
