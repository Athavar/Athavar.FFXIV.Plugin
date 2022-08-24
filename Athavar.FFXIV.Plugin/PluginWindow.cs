// <copyright file="PluginWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Numerics;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Game.Text;
using Dalamud.Interface.Windowing;
using ImGuiNET;

/// <summary>
///     The main <see cref="Window" /> of the plugin.
/// </summary>
internal class PluginWindow : Window
{
    private readonly IModuleManager manager;
    private readonly ILocalizerManager localizerManager;

    private readonly string[] languages = Enum.GetNames<Language>();
    private readonly Configuration configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginWindow" /> class.
    /// </summary>
    /// <param name="localizerManager"><see cref="ILocalizerManager" /> added by DI.</param>
    /// <param name="manager"><see cref="IModuleManager" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    public PluginWindow(ILocalizerManager localizerManager, IModuleManager manager, Configuration configuration)
        : base($"{Plugin.PluginName}")
    {
        this.manager = manager;
        this.localizerManager = localizerManager;
        this.configuration = configuration;

        this.Size = new Vector2(525, 600);
        this.SizeCondition = ImGuiCond.FirstUseEver;
        this.RespectCloseHotkey = false;

#if DEBUG
        this.Toggle();
#endif
    }

    /// <inheritdoc />
    public override void PreDraw() => ImGui.PushStyleColor(ImGuiCol.ResizeGrip, 0);

    /// <inheritdoc />
    public override void PostDraw() => ImGui.PopStyleColor();

    /// <inheritdoc />
    public override void Draw()
    {
        ImGui.BeginTabBar("##tabBar");

        this.DrawSettingTab();

        this.manager.Draw();

        ImGui.EndTabBar();
        ImGui.End();
    }

    private void DrawSettingTab()
    {
        using var raii = new ImGuiRaii();
        if (!raii.Begin(() => ImGui.BeginTabItem(this.localizerManager.Localize("Settings")), ImGui.EndTabItem))
        {
            return;
        }

        var change = false;
        var config = this.configuration;

        // ToolTip setting
        var value = config.ShowToolTips;
        ImGui.TextUnformatted(this.localizerManager.Localize("Tooltips"));
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

        if (ImGui.CollapsingHeader(this.localizerManager.Localize("Chat")))
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
            if (ImGui.Combo(this.localizerManager.Localize("Normal chat channel"), ref current, names, names.Length))
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
            if (ImGui.Combo(this.localizerManager.Localize("Error chat channel"), ref currentError, names, names.Length))
            {
                config.ChatType = chatTypes[currentError];
                change = true;
            }
        }

        if (ImGui.CollapsingHeader(this.localizerManager.Localize("Modules")))
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
    }
}