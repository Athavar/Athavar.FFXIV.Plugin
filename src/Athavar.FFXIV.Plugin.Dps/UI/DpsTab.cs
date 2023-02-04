// <copyright file="DpsTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.UI.Config;
using Athavar.FFXIV.Plugin.OpcodeWizard;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal class DpsTab : Tab, IDpsTab
{
    private const float NavBarHeight = 40;
    private readonly MeterManager meterManager;
    private readonly IDalamudServices dalamudServices;
    private readonly IOpcodeManager opcodeManager;

    private readonly Stack<IConfigurable> configStack = new();

    private readonly Opcode[] requiredOpcodes = { Opcode.ActionEffect1, Opcode.ActionEffect8, Opcode.ActionEffect16, Opcode.ActionEffect24, Opcode.ActionEffect32, Opcode.EffectResult, Opcode.ActorControl };

    private bool back;
    private bool home;
    private string inputName = string.Empty;

    private bool verifyOpcodes;

    public DpsTab(IServiceProvider provider, IDalamudServices dalamudServices, MeterManager meterManager, IOpcodeManager opcodeManager)
    {
        this.meterManager = meterManager;
        this.dalamudServices = dalamudServices;
        this.opcodeManager = opcodeManager;
        this.configStack.Push(ActivatorUtilities.CreateInstance<DpsConfigPage>(provider));
    }

    public override string Name => DpsModule.ModuleName;

    public override string Identifier => "dps";

    public override string Title => string.Join("  >  ", this.configStack.Reverse().Select(c => c.Name));

    public override void Draw()
    {
        if (!this.CheckRequiredOpcodes())
        {
            return;
        }

        if (!this.configStack.Any())
        {
            return;
        }

        var configItem = this.configStack.Peek();
        var spacing = ImGui.GetStyle().ItemSpacing;
        var size = ImGui.GetContentRegionAvail() - (spacing * 2);
        var drawNavBar = this.configStack.Count > 1;

        if (drawNavBar)
        {
            size -= new Vector2(0, NavBarHeight + spacing.Y);
        }

        IConfigPage? openPage = null;
        if (ImGui.BeginTabBar($"##{this.Name}"))
        {
            foreach (var page in configItem.GetConfigPages())
            {
                if (ImGui.BeginTabItem($"{page.Name}##{this.Name}"))
                {
                    openPage = page;
                    page.DrawConfig(size.AddY(-ImGui.GetCursorPosY()), spacing.X, spacing.Y);
                    ImGui.EndTabItem();
                }
            }

            ImGui.EndTabBar();
        }

        if (drawNavBar)
        {
            this.DrawNavBar(openPage, size, spacing.X);
        }

        this.PostDraw();
    }

    public void PushConfig(IConfigurable configItem)
    {
        this.configStack.Push(configItem);
        this.inputName = configItem.Name;
    }

    private bool CheckRequiredOpcodes()
    {
        if (this.verifyOpcodes)
        {
            return true;
        }

        List<Opcode> missing = new();
        foreach (var requiredOpcode in this.requiredOpcodes)
        {
            var code = this.opcodeManager.GetOpcode(requiredOpcode);
            if (code == default)
            {
                missing.Add(requiredOpcode);
            }
        }

        if (!missing.Any())
        {
            this.verifyOpcodes = true;
            return true;
        }

        ImGui.Text("Missing Opcodes. Please use the OpcodeWizard-Module do find following opcode:");
        foreach (var opcode in missing)
        {
            ImGui.TextColored(ImGuiColors.DalamudRed, opcode.AsText());
        }

        return false;
    }

    private void DrawNavBar(IConfigPage? openPage, Vector2 size, float padX)
    {
        var buttonSize = 40;
        float textInputWidth = 150;

        if (ImGui.BeginChild($"##{this.Name}_NavBar", new Vector2(size.X, NavBarHeight), true))
        {
            if (ImGuiEx.IconButton(FontAwesomeIcon.LongArrowAltLeft, "Back", buttonSize))
            {
                this.back = true;
            }

            ImGui.SameLine();

            if (this.configStack.Count > 2)
            {
                if (ImGuiEx.IconButton(FontAwesomeIcon.Home, "Home", buttonSize))
                {
                    this.home = true;
                }

                ImGui.SameLine();
            }
            else
            {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 40 + padX);
            }

            // calculate empty horizontal space based on size of buttons and text box
            var offset = size.X - (buttonSize * 5) - textInputWidth - (padX * 7);

            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);

            if (ImGuiEx.IconButton(FontAwesomeIcon.UndoAlt, $"Reset {openPage?.Name} to Defaults", buttonSize))
            {
                this.Reset(openPage);
            }

            ImGui.SameLine();

            ImGui.PushItemWidth(textInputWidth);
            if (ImGui.InputText("##Input", ref this.inputName, 64, ImGuiInputTextFlags.EnterReturnsTrue))
            {
                this.Rename(this.inputName);
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.SetTooltip("Rename");
            }

            ImGui.PopItemWidth();
            ImGui.SameLine();

            if (ImGuiEx.IconButton(FontAwesomeIcon.Upload, $"Export {openPage?.Name}", buttonSize))
            {
                this.Export(openPage?.GetConfig());
            }

            ImGui.SameLine();

            if (ImGuiEx.IconButton(FontAwesomeIcon.Download, $"Import {openPage?.Name}", buttonSize))
            {
                this.Import();
            }
        }

        ImGui.EndChild();
    }

    private void PostDraw()
    {
        if (this.home)
        {
            while (this.configStack.Count > 1)
            {
                this.configStack.Pop();
            }
        }
        else if (this.back && this.configStack.Count > 1)
        {
            this.configStack.Pop();
        }

        if ((this.home || this.back) && this.configStack.Count > 1)
        {
            this.inputName = this.configStack.Peek().Name;
            this.meterManager.Save();
        }

        this.home = false;
        this.back = false;
    }

    private void Reset(IConfigPage? openPage)
    {
        if (openPage is not null)
        {
            this.configStack.Peek().ImportConfig(openPage.GetDefault());
        }
    }

    private void Export(IConfig? openPage)
    {
        if (openPage is not null)
        {
            var exportString = openPage.GetExportString();

            if (exportString is not null)
            {
                ImGui.SetClipboardText(exportString);
                this.DrawNotification("Export string copied to clipboard.");
            }
            else
            {
                this.DrawNotification("Failed to Export!", NotificationType.Error);
            }
        }
    }

    private void DrawNotification(string message, NotificationType type = NotificationType.Success)
        => this.dalamudServices.PluginInterface.UiBuilder
           .AddNotification(message, DpsModule.ModuleName, type);

    private void Import()
    {
        var importString = ImGui.GetClipboardText();
        IConfig? page = BaseConfig.GetFromImportString(importString);

        if (page is not null)
        {
            this.configStack.Peek().ImportConfig(page);
        }
    }

    private void Rename(string name)
    {
        if (this.configStack.Any())
        {
            this.configStack.Peek().Name = name;
        }
    }
}