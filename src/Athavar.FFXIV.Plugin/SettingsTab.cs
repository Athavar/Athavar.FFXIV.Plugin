// <copyright file="SettingsTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Game.Text;
using Dalamud.Interface.Colors;
using ImGuiNET;

internal sealed partial class SettingsTab : Tab
{
    private readonly PluginWindow window;
    private readonly IDalamudServices dalamudServices;
    private readonly IModuleManager manager;
    private readonly ILocalizeManager localizeManager;
    private readonly string[] languages = Enum.GetNames<Language>();
    private readonly CommonConfiguration configuration;
    private readonly IServiceProvider serviceProvider;

    public SettingsTab(PluginWindow window, IDalamudServices dalamudServices, IModuleManager manager, ILocalizeManager localizeManager, CommonConfiguration configuration, IServiceProvider serviceProvider)
    {
        this.window = window;
        this.dalamudServices = dalamudServices;
        this.manager = manager;
        this.localizeManager = localizeManager;
        this.configuration = configuration;
        this.serviceProvider = serviceProvider;
    }

    public override string Name => "Settings";

    public override string Identifier => "settings";

    public override void Draw()
    {
        var change = false;
        var config = this.configuration;

        // ToolTip setting
        var value = config.ShowToolTips;
        ImGui.TextUnformatted(this.localizeManager.Localize("Tooltips"));
        ImGui.AlignTextToFramePadding();
        ImGui.SameLine();
        if (ImGui.Checkbox("##hideTooltipsOnOff", ref value))
        {
            config.ShowToolTips = value;
            change = true;
        }

        value = config.ShowLaunchButton;
        ImGui.TextUnformatted(this.localizeManager.Localize("Launch Button"));
        ImGui.AlignTextToFramePadding();
        ImGui.SameLine();
        if (ImGui.Checkbox("##showLaunchButtonOnOff", ref value))
        {
            config.ShowLaunchButton = value;
            change = true;
            if (value)
            {
                this.window.LaunchButton.AddEntry();
            }
            else
            {
                this.window.LaunchButton.RemoveEntry();
            }
        }

        /*
        // Language setting
        var selectedLanguage = (int)config.Language;
        ImGui.TextUnformatted(this.localizerManager.Localize("Language:"));
        if (config.ShowToolTips && ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(this.localizerManager.Localize("Change the UI Language."));
        }

        ImGui.SameLine();
        ImGui.SetNextItemWidth(200);
        if (ImGui.Combo("##hideLangSetting", ref selectedLanguage, this.languages, this.languages.Length))
        {
            this.localizerManager.ChangeLanguage(config.Language = (Language)selectedLanguage);
            change = true;
        }*/

        if (ImGui.CollapsingHeader(this.localizeManager.Localize("Chat")))
        {
            var names = Enum.GetNames<XivChatType>();
            var chatTypes = Enum.GetValues<XivChatType>();

            var current = Array.IndexOf(chatTypes, config.ChatType);
            if (current == -1)
            {
                current = Array.IndexOf(chatTypes, config.ChatType = XivChatType.Echo);
                change = true;
            }

            ImGui.SetNextItemWidth(200f);
            if (ImGui.Combo(this.localizeManager.Localize("Normal chat channel"), ref current, names, names.Length))
            {
                config.ChatType = chatTypes[current];
                change = true;
            }

            var currentError = Array.IndexOf(chatTypes, config.ErrorChatType);
            if (currentError == -1)
            {
                currentError = Array.IndexOf(chatTypes, config.ErrorChatType = XivChatType.Urgent);
                change = true;
            }

            ImGui.SetNextItemWidth(200f);
            if (ImGui.Combo(this.localizeManager.Localize("Error chat channel"), ref currentError, names, names.Length))
            {
                config.ErrorChatType = chatTypes[currentError];
                change = true;
            }
        }

        if (ImGui.BeginTable("##modules", 4))
        {
            ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Tab", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var index = 0;
            foreach (var module in this.manager.GetModuleData())
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                // enabled row
                var val = module.Enabled;
                if (ImGui.Checkbox("###enabled" + index, ref val))
                {
                    module.Enabled = val;
                }

                // name row
                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted(module.Name);

                // tab row
                ImGui.TableSetColumnIndex(2);
                if (module.HasTab)
                {
                    val = module.TabEnabled;
                    if (ImGui.Checkbox("###tab" + index, ref val))
                    {
                        module.TabEnabled = val;
                    }
                }

                // status row
                ImGui.TableSetColumnIndex(3);
                if (module.Loaded)
                {
                    ImGui.TextUnformatted("Active");
                }
                else if (module.Enabled)
                {
                    ImGui.TextUnformatted("Loading");
                }
                else if (module.LoadingError is not null)
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
                    ImGui.TextUnformatted("Error");
                    ImGui.PopStyleColor();
                }
                else
                {
                    ImGui.TextUnformatted("Disabled");
                }

                index++;
            }

            ImGui.EndTable();
        }

        if (change)
        {
            this.configuration.Save();
        }

#if DEBUG
        {
            using var g = ImGuiRaii.NewGroup();
            if (ImGui.CollapsingHeader(this.localizeManager.Localize("Test") + "##test-collapse"))
            {
                this.DrawTest();
            }
        }
#endif
    }
}