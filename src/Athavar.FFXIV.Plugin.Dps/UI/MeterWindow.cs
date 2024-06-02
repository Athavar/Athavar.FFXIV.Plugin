// <copyright file="MeterWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Dps.UI;

using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data.Encounter;
using Athavar.FFXIV.Plugin.Dps.UI.Config;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal sealed class MeterWindow : IConfigurable
{
    private readonly MeterManager meterManager;
    private readonly ICommandInterface ci;
    private readonly EncounterManager encounterManager;

    private readonly Encounter? previewEvent = null;

    private bool lastFrameWasUnlocked;

    private bool lastFrameWasDragging;

    private bool lastFrameWasPreview;

    private bool lastFrameWasCombat;

    private bool unlocked;

    private bool hovered;

    private bool dragging;

    private bool locked;

    private int territoryIndex = -2;

    private int encounterIndex = -1;

    private int scrollPosition;

    private DateTime? lastSortedTimestamp;
    private MeterDataType? lastSortedMeterDataType;

    private List<BaseCombatant> lastSortedCombatants = new();

    public MeterWindow(MeterConfig config, IServiceProvider provider, MeterManager meterManager)
    {
        this.ID = $"DpsModule_MeterWindow_{Guid.NewGuid()}";
        this.Config = config;
        this.ci = provider.GetRequiredService<ICommandInterface>();
        this.encounterManager = provider.GetRequiredService<EncounterManager>();
        this.meterManager = meterManager;

        var utils = provider.GetRequiredService<Utils>();
        var fontManager = provider.GetRequiredService<IFontsManager>();
        var iconManager = provider.GetRequiredService<IIconManager>();
        var dalamudServices = provider.GetRequiredService<IDalamudServices>();

        this.GeneralConfigPage = new GeneralConfigPage(this);
        this.HeaderConfigPage = new HeaderConfigPage(this, fontManager);
        this.BarConfigPage = new BarConfigPage(this, dalamudServices, utils, fontManager, iconManager);
        this.BarColorsConfigPage = new BarColorsConfigPage(this);
        this.VisibilityConfigPage = new VisibilityConfigPage(this, this.ci);
    }

    public MeterConfig Config { get; }

    public bool Enabled => this.Config.GeneralConfig.Enabled;

    public string ID { get; init; }

    public string Name
    {
        get => this.Config.Name;
        set => this.Config.Name = value;
    }

    private GeneralConfigPage GeneralConfigPage { get; }

    private HeaderConfigPage HeaderConfigPage { get; }

    private BarConfigPage BarConfigPage { get; }

    private BarColorsConfigPage BarColorsConfigPage { get; }

    private VisibilityConfigPage VisibilityConfigPage { get; }

    public static MeterWindow GetDefaultMeter(string name, IServiceProvider provider, MeterManager meterManager)
    {
        var config = new MeterConfig
        {
            Name = name,
        };

        var newMeter = new MeterWindow(config, provider, meterManager);
        return newMeter;
    }

    public IEnumerable<IConfigPage> GetConfigPages()
    {
        yield return this.GeneralConfigPage;
        yield return this.HeaderConfigPage;
        yield return this.BarConfigPage;
        yield return this.BarColorsConfigPage;
        yield return this.VisibilityConfigPage;
    }

    public void ImportConfig(IConfig c)
    {
        switch (c)
        {
            case GeneralConfig generalConfig:
                this.Config.GeneralConfig = generalConfig;
                break;
            case HeaderConfig headerConfig:
                this.Config.HeaderConfig = headerConfig;
                break;
            case BarConfig barConfig:
                this.Config.BarConfig = barConfig;
                break;
            case BarColorsConfig colorsConfig:
                this.Config.BarColorsConfig = colorsConfig;
                break;
            case VisibilityConfig visibilityConfig:
                this.Config.VisibilityConfig = visibilityConfig;
                break;
        }
    }

    public void Clear()
    {
        this.lastSortedCombatants = new List<BaseCombatant>();
        this.lastSortedTimestamp = null;
    }

    public void CacheReset() => this.BarConfigPage.CacheReset();

    public void Save() => this.meterManager.Save();

    public void Draw(Vector2 pos)
    {
        if (!this.GeneralConfigPage.Preview && !this.VisibilityConfigPage.IsVisible())
        {
            return;
        }

        var generalConfig = this.Config.GeneralConfig;
        var localPos = pos + generalConfig.Position;
        var size = generalConfig.Size ??= GeneralConfig.SizeDefault;

        if (ImGui.IsMouseHoveringRect(localPos, localPos + size))
        {
            this.scrollPosition -= (int)ImGui.GetIO().MouseWheel;

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && !generalConfig.Preview)
            {
                ImGui.OpenPopup($"{this.ID}_ContextMenu", ImGuiPopupFlags.MouseButtonRight);
            }
        }

        if (this.DrawContextMenu($"{this.ID}_ContextMenu", out var territory, out var encounter))
        {
            this.territoryIndex = territory;
            this.encounterIndex = encounter;
            this.lastSortedTimestamp = null;
            this.lastSortedCombatants = new List<BaseCombatant>();
            this.scrollPosition = 0;
        }

        var combat = this.ci.IsInCombat();
        if (generalConfig.ReturnToCurrent && !this.lastFrameWasCombat && combat)
        {
            this.territoryIndex = -1;
            this.encounterIndex = -1;
        }

        this.UpdateDragData(localPos, size, generalConfig.Lock);
        var needsInput = !generalConfig.ClickThrough;
        ImGuiEx.DrawInWindow($"##{this.ID}", localPos, size, needsInput, this.locked || generalConfig.Lock, drawList =>
        {
            if (this.unlocked)
            {
                if (this.lastFrameWasDragging)
                {
                    localPos = ImGui.GetWindowPos();
                    var newPos = localPos - pos;
                    if (generalConfig.Position != newPos)
                    {
                        generalConfig.Position = newPos;
                        this.meterManager.Save();
                    }

                    size = ImGui.GetWindowSize();
                    if (generalConfig.Size != size)
                    {
                        generalConfig.Size = size;
                        this.meterManager.Save();
                    }
                }
            }

            if (generalConfig.ShowBorder)
            {
                var borderPos = localPos;
                var borderSize = size;
                var headerConfig = this.Config.HeaderConfig;
                if (generalConfig.BorderAroundBars &&
                    headerConfig.ShowHeader)
                {
                    borderPos = borderPos.AddY(headerConfig.HeaderHeight);
                    borderSize = borderSize.AddY(-headerConfig.HeaderHeight);
                }

                for (var i = 0; i < generalConfig.BorderThickness; i++)
                {
                    var offset = new Vector2(i, i);
                    drawList.AddRect(borderPos + offset, (borderPos + borderSize) - offset, generalConfig.BorderColor.Base);
                }

                localPos += Vector2.One * generalConfig.BorderThickness;
                size -= Vector2.One * generalConfig.BorderThickness * 2;
            }

            if (this.GeneralConfigPage.Preview && !this.lastFrameWasPreview)
            {
                // this._previewEvent = Encounter.GetTestData();
                this.GeneralConfigPage.Preview = false;
            }

            var encounter = this.GeneralConfigPage.Preview ? this.previewEvent : this.encounterManager.GetEncounter(this.territoryIndex, this.encounterIndex);

            (localPos, size) = this.HeaderConfigPage.DrawHeader(localPos, size, encounter, drawList);
            drawList.AddRectFilled(localPos, localPos + size, generalConfig.BackgroundColor.Base);
            this.DrawBars(drawList, localPos, size, encounter);
        });

        this.lastFrameWasUnlocked = this.unlocked;
        this.lastFrameWasPreview = this.GeneralConfigPage.Preview;
        this.lastFrameWasCombat = combat;
    }

    // Dont ask
    private void UpdateDragData(Vector2 pos, Vector2 size, bool locked)
    {
        this.unlocked = !locked;
        this.hovered = ImGui.IsMouseHoveringRect(pos, pos + size);
        this.dragging = this.lastFrameWasDragging && ImGui.IsMouseDown(ImGuiMouseButton.Left);
        this.locked = (this.unlocked && !this.lastFrameWasUnlocked || !this.hovered) && !this.dragging;
        this.lastFrameWasDragging = this.hovered || this.dragging;
    }

    private void DrawBars(ImDrawListPtr drawList, Vector2 localPos, Vector2 size, BaseEncounter? actEvent)
    {
        if (actEvent is null)
        {
            return;
        }

        var dataType = this.Config.GeneralConfig.DataType;
        var sortedCombatants = this.GetSortedCombatants(actEvent, dataType);

        if (sortedCombatants.Any())
        {
            var barConfig = this.Config.BarConfig;

            var barCount = Math.Clamp(sortedCombatants.Count, barConfig.MinBarCount, barConfig.BarCount);

            var top = sortedCombatants.FirstOrDefault()?.GetMeterData(dataType) ?? 0;

            // check and set limits of scrollPosition
            this.scrollPosition = Math.Clamp(this.scrollPosition, 0, Math.Max(0, sortedCombatants.Count - barCount));

            var maxIndex = Math.Min(this.scrollPosition + barCount, sortedCombatants.Count);
            for (var i = this.scrollPosition; i < maxIndex; i++)
            {
                var combatant = sortedCombatants[i];
                combatant.Rank = (i + 1).ToString();

                var current = combatant.GetMeterData(dataType);

                var barColor = barConfig.BarColor;
                var jobColor = this.BarColorsConfigPage.GetColor(combatant.Job);
                localPos = this.BarConfigPage.DrawBar(drawList, localPos, size, combatant, jobColor, barColor, barCount, top, current);
            }
        }
    }

    private bool DrawContextMenu(string popupId, out int selectedTerritoryIndex, out int selectedEncounterIndex)
    {
        selectedTerritoryIndex = -1;
        selectedEncounterIndex = -1;
        var selected = false;

        if (ImGui.BeginPopup(popupId))
        {
            /*if (!ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                ImGui.SetKeyboardFocusHere(0);
            }*/

            if (ImGui.Selectable("Current Data"))
            {
                selected = true;
            }

            var territories = this.encounterManager.EncounterHistory;
            if (territories.Count > 0)
            {
                ImGui.Separator();
            }

            for (var i = 0; i < territories.Count; i++)
            {
                var territory = territories[i];
                if (ImGui.BeginMenu(territory.TitleStart))
                {
                    if (ImGui.Selectable($"All — {territory.Duration:hh\\mm\\:ss\\.ff}"))
                    {
                        ImGui.CloseCurrentPopup();
                        selectedTerritoryIndex = i;
                        selected = true;
                    }

                    var encounters = territory.Encounters;
                    for (var j = 0; j < encounters.Count; j++)
                    {
                        var encounter = encounters[j];
                        if (ImGui.Selectable($"{encounter.Title} — {encounter.Duration:mm\\:ss\\.ff}"))
                        {
                            ImGui.CloseCurrentPopup();
                            selectedTerritoryIndex = i;
                            selectedEncounterIndex = j;
                            selected = true;
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            ImGui.Separator();
            if (ImGui.Selectable("Clear Data"))
            {
                this.encounterManager.Clear();
                selected = true;
            }

            if (ImGui.Selectable("Configure"))
            {
                this.meterManager.ConfigureMeter(this);
                selected = true;
            }

            ImGui.EndPopup();
        }

        return selected;
    }

    private List<BaseCombatant> GetSortedCombatants(BaseEncounter encounter, MeterDataType dataType)
    {
        if (this.lastSortedTimestamp.HasValue && this.lastSortedTimestamp.Value == encounter.LastEvent &&
            this.lastSortedMeterDataType == dataType && !this.GeneralConfigPage.Preview)
        {
            return this.lastSortedCombatants;
        }

        var sortedCombatants = encounter.GetAllyCombatants()
           .Where(c => c.GetMeterData(dataType) > 0)
           .ToList();

        sortedCombatants.Sort((x, y) => (int)(y.GetMeterData(dataType) - x.GetMeterData(dataType)));

        this.lastSortedTimestamp = encounter.LastEvent;
        this.lastSortedMeterDataType = dataType;
        this.lastSortedCombatants = sortedCombatants;
        return sortedCombatants;
    }
}