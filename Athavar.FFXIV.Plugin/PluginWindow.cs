// <copyright file="PluginWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Diagnostics;
using System.Numerics;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;

/// <summary>
///     The main <see cref="Window" /> of the plugin.
/// </summary>
internal class PluginWindow : Window, IDisposable, IPluginWindow
{
    private readonly IModuleManager manager;

    private readonly TabBarHandler tabBarHandler = new("athavar-toolbox");

    private readonly SettingsTab settingsTab;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginWindow" /> class.
    /// </summary>
    /// <param name="localizeManager"><see cref="ILocalizeManager" /> added by DI.</param>
    /// <param name="manager"><see cref="IModuleManager" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    public PluginWindow(ILocalizeManager localizeManager, IModuleManager manager, Configuration configuration)
        : base("ConfigRoot###mainWindow")
    {
        this.manager = manager;
        this.settingsTab = new SettingsTab(this.manager, localizeManager, configuration);
        this.tabBarHandler.Add(this.settingsTab);

        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.PositionCondition = ImGuiCond.Appearing;
        this.RespectCloseHotkey = false;

        this.manager.StateChange += this.OnModuleStateChange;

#if DEBUG
        this.Toggle();
#endif
    }

    /// <inheritdoc />
    public override void PreDraw()
    {
        ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);
        this.WindowName = $"{Plugin.PluginName} - {this.tabBarHandler.GetTabTitle()}###mainWindow";
        ImGui.SetNextWindowSize(this.Size.GetValueOrDefault());
    }

    /// <inheritdoc />
    public override void PostDraw()
    {
        ImGui.PopStyleColor();

        this.Position = ImGui.GetWindowPos();
    }

    /// <inheritdoc />
    public override void Draw() => this.tabBarHandler.Draw();

    /// <inheritdoc />
    public void Dispose() => this.manager.StateChange -= this.OnModuleStateChange;

    private void OnModuleStateChange(Module module, bool init)
    {
        if (module.Enabled)
        {
            if (module.Tab is null)
            {
                return;
            }

            this.tabBarHandler.Add(module.Tab);
        }
        else if (!init)
        {
            if (module.Tab is null)
            {
                return;
            }

            this.tabBarHandler.Remove(module.Tab);
        }
    }

    private class SettingsTab : Tab
    {
        private readonly IModuleManager manager;
        private readonly ILocalizeManager localizeManager;
        private readonly string[] languages = Enum.GetNames<Language>();
        private readonly Configuration configuration;

#if DEBUG
        private readonly Stopwatch stopwatch = new();

        private long totalCount;
        private int totalLoop;
#endif

        public SettingsTab(IModuleManager manager, ILocalizeManager localizeManager, Configuration configuration)
        {
            this.manager = manager;
            this.localizeManager = localizeManager;
            this.configuration = configuration;
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
                    config.ChatType = chatTypes[currentError];
                    change = true;
                }
            }

            if (ImGui.CollapsingHeader(this.localizeManager.Localize("Modules")))
            {
                foreach (var module in this.manager.GetModuleNames())
                {
                    var val = this.manager.IsEnables(module);
                    if (ImGui.Checkbox(module, ref val))
                    {
                        this.manager.Enable(module, val);
                        change = true;
                    }
                }
            }

            if (change)
            {
                this.configuration.Save();
            }

#if DEBUG
            if (ImGui.CollapsingHeader(this.localizeManager.Localize("Test")))
            {
                this.stopwatch.Restart();

                for (var i = 0; i < 100; i++)
                {
                    ImGui.TextUnformatted("Hallo World");
                }

                if (this.totalLoop > 1)
                {
                    this.totalCount += this.stopwatch.ElapsedTicks;

                    ImGui.TextUnformatted($"{this.totalCount / (this.totalLoop - 1)}ticks");
                }

                this.totalLoop++;
            }
#endif
        }
    }
}