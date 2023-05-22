// <copyright file="ProfileListConfigPage.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal sealed class ProfileListConfigPage : IConfigPage
{
    private const float MenuBarHeight = 40;

    private readonly MeterManager meterManager;
    private readonly IServiceProvider provider;

    [JsonIgnore]
    private string input = string.Empty;

    public ProfileListConfigPage(MeterManager meterManager, IServiceProvider provider)
    {
        this.meterManager = meterManager;
        this.provider = provider;
    }

    public string Name => "Profiles";

    private IReadOnlyList<MeterWindow> Meters => this.meterManager.Meters;

    public IConfig GetDefault() => new DummyConfig();

    public IConfig GetConfig() => new DummyConfig();

    public void DrawConfig(Vector2 size, float padX, float padY)
    {
        ImGui.TextWrapped("Note: DoT&HoT are assigned to players by potency of current target status effects. Buffs and gear are not taken into account.");
        this.DrawCreateMenu(size, padX);
        this.DrawMeterTable(size.AddY(-padY), padX);
    }

    public void ToggleMeter(int meterIndex, bool? toggle = null)
    {
        if (meterIndex >= 0 && meterIndex < this.Meters.Count)
        {
            this.Meters[meterIndex].Config.VisibilityConfig.AlwaysHide = !toggle ?? !this.Meters[meterIndex].Config.VisibilityConfig.AlwaysHide;
        }
    }

    public void ToggleClickThrough(int meterIndex)
    {
        if (meterIndex >= 0 && meterIndex < this.Meters.Count)
        {
            this.Meters[meterIndex].Config.GeneralConfig.ClickThrough ^= true;
        }
    }

    private void DrawCreateMenu(Vector2 size, float padX)
    {
        const int buttonSize = 40;
        var textInputWidth = size.X - (buttonSize * 2) - (padX * 4);

        if (ImGui.BeginChild("##Buttons", new Vector2(size.X, MenuBarHeight), true))
        {
            ImGui.PushItemWidth(textInputWidth);
            ImGui.InputTextWithHint("##Input", "Profile Name/Import String", ref this.input, 10000);
            ImGui.PopItemWidth();

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Plus, "Create new Meter", buttonSize))
            {
                this.CreateMeter(this.input);
            }

            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.Download, "Import new Meter", buttonSize))
            {
                this.ImportMeter(this.input);
            }

            ImGui.PopItemWidth();
        }

        ImGui.EndChild();
    }

    private void DrawMeterTable(Vector2 size, float padX)
    {
        var flags =
            ImGuiTableFlags.RowBg |
            ImGuiTableFlags.Borders |
            ImGuiTableFlags.BordersOuter |
            ImGuiTableFlags.BordersInner |
            ImGuiTableFlags.ScrollY |
            ImGuiTableFlags.NoSavedSettings;

        if (ImGui.BeginTable("##Profile_Table", 3, flags, size with { Y = size.Y - MenuBarHeight }))
        {
            const int buttonSize = 30;
            var actionsWidth = (buttonSize * 3) + (padX * 2);

            ImGui.TableSetupColumn("   #", ImGuiTableColumnFlags.WidthFixed, 18, 0);
            ImGui.TableSetupColumn("Profile Name", ImGuiTableColumnFlags.WidthStretch, 0, 1);
            ImGui.TableSetupColumn("Actions", ImGuiTableColumnFlags.WidthFixed, actionsWidth, 2);

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            for (var i = 0; i < this.Meters.Count; i++)
            {
                var meter = this.Meters[i];

                if (!string.IsNullOrEmpty(this.input) &&
                    !meter.Name.Contains(this.input, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                ImGui.PushID(i.ToString());
                ImGui.TableNextRow(ImGuiTableRowFlags.None, 28);

                if (ImGui.TableSetColumnIndex(0))
                {
                    var num = $"  {i + 1}.";
                    var columnWidth = ImGui.GetColumnWidth();
                    var cursorPos = ImGui.GetCursorPos();
                    var textSize = ImGui.CalcTextSize(num);
                    ImGui.SetCursorPos(new Vector2((cursorPos.X + columnWidth) - textSize.X, cursorPos.Y + 3f));
                    ImGui.Text(num);
                }

                if (ImGui.TableSetColumnIndex(1))
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 3f);
                    ImGui.Text(meter.Name);
                }

                if (ImGui.TableSetColumnIndex(2))
                {
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1f);
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Pen, "Edit", buttonSize))
                    {
                        this.EditMeter(meter);
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Upload, "Export", buttonSize))
                    {
                        this.ExportMeter(meter);
                    }

                    ImGui.SameLine();
                    if (ImGuiEx.IconButton(FontAwesomeIcon.Trash, "Delete", buttonSize))
                    {
                        this.DeleteMeter(meter);
                    }
                }
            }

            ImGui.EndTable();
        }
    }

    private void CreateMeter(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            this.meterManager.AddMeter(MeterWindow.GetDefaultMeter(name, this.provider, this.meterManager));
        }

        this.input = string.Empty;
    }

    private void EditMeter(MeterWindow meter) => this.meterManager.ConfigureMeter(meter);

    private void DeleteMeter(MeterWindow meter) => this.meterManager.DeleteMeter(meter);

    private void ImportMeter(string input)
    {
        var importString = input;
        if (string.IsNullOrWhiteSpace(importString))
        {
            importString = ImGui.GetClipboardText();
        }

        var newMeter = BaseConfig.GetFromImportString(importString);
        if (newMeter is not null && newMeter is MeterConfig meterConfig)
        {
            this.meterManager.AddMeter(new MeterWindow(meterConfig, this.provider, this.meterManager));
        }
        else
        {
            this.DrawNotification("Failed to Import Meter!", NotificationType.Error);
        }

        this.input = string.Empty;
    }

    private void ExportMeter(MeterWindow meter)
    {
        var exportString = meter.Config.GetExportString();
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

    private void DrawNotification(string message, NotificationType type = NotificationType.Success)
        => this.provider.GetRequiredService<IDalamudServices>().PluginInterface.UiBuilder
           .AddNotification(message, DpsModule.ModuleName, type);
}