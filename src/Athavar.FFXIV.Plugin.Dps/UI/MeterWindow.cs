// <copyright file="MeterWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.UI;

using System.Numerics;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Dps.Data;
using Athavar.FFXIV.Plugin.Dps.UI.Config;
using Dalamud.Game.ClientState.Objects.Enums;
using ImGuiNET;
using Microsoft.Extensions.DependencyInjection;

internal class MeterWindow : IConfigurable
{
    private readonly MeterManager meterManager;
    private readonly ICommandInterface ci;
    private readonly EncounterManager encounterManager;

    [JsonIgnore]
    private readonly Encounter? previewEvent = null;

    [JsonIgnore]
    private bool lastFrameWasUnlocked;

    [JsonIgnore]
    private bool lastFrameWasDragging;

    [JsonIgnore]
    private bool lastFrameWasPreview;

    [JsonIgnore]
    private bool lastFrameWasCombat;

    [JsonIgnore]
    private bool unlocked;

    [JsonIgnore]
    private bool hovered;

    [JsonIgnore]
    private bool dragging;

    [JsonIgnore]
    private bool locked;

    [JsonIgnore]
    private int eventIndex = -1;

    [JsonIgnore]
    private int scrollPosition;

    [JsonIgnore]
    private DateTime? lastSortedTimestamp;

    [JsonIgnore]
    private List<Combatant> lastSortedCombatants = new();

    public MeterWindow(MeterConfig config, IServiceProvider provider)
        : this(config, provider, provider.GetRequiredService<MeterManager>())
    {
    }

    public MeterWindow(MeterConfig config, IServiceProvider provider, MeterManager meterManager)
    {
        this.ID = $"DpsModule_MeterWindow_{Guid.NewGuid()}";
        this.Config = config;
        this.ci = provider.GetRequiredService<ICommandInterface>();
        this.encounterManager = provider.GetRequiredService<EncounterManager>();
        this.meterManager = meterManager;

        var fontManager = provider.GetRequiredService<IFontsManager>();
        var iconManager = provider.GetRequiredService<IIconManager>();

        this.GeneralConfigPage = new GeneralConfigPage(config);
        this.HeaderConfigPage = new HeaderConfigPage(config, fontManager);
        this.BarConfigPage = new BarConfigPage(config, fontManager, iconManager);
        this.BarColorsConfigPage = new BarColorsConfigPage(config);
        this.VisibilityConfigPage = new VisibilityConfigPage(config, this.ci);
    }

    public MeterConfig Config { get; }

    [JsonIgnore]
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

    public static MeterWindow GetDefaultMeter(string name, IServiceProvider provider)
    {
        var config = new MeterConfig
        {
            Name = name,
        };

        var newMeter = new MeterWindow(config, provider);
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
        this.lastSortedCombatants = new List<Combatant>();
        this.lastSortedTimestamp = null;
    }

    public void Draw(Vector2 pos)
    {
        if (!this.GeneralConfigPage.Preview && !this.VisibilityConfigPage.IsVisible())
        {
            return;
        }

        var generalConfig = this.Config.GeneralConfig;
        var localPos = pos + generalConfig.Position;
        var size = generalConfig.Size;

        if (ImGui.IsMouseHoveringRect(localPos, localPos + size))
        {
            this.scrollPosition -= (int)ImGui.GetIO().MouseWheel;

            if (ImGui.IsMouseClicked(ImGuiMouseButton.Right) && !generalConfig.Preview)
            {
                ImGui.OpenPopup($"{this.ID}_ContextMenu", ImGuiPopupFlags.MouseButtonRight);
            }
        }

        if (this.DrawContextMenu($"{this.ID}_ContextMenu", out var index))
        {
            this.eventIndex = index;
            this.lastSortedTimestamp = null;
            this.lastSortedCombatants = new List<Combatant>();
            this.scrollPosition = 0;
        }

        var combat = this.ci.IsInCombat();
        if (generalConfig.ReturnToCurrent && !this.lastFrameWasCombat && combat)
        {
            this.eventIndex = -1;
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
                    generalConfig.Position = localPos - pos;

                    size = ImGui.GetWindowSize();
                    generalConfig.Size = size;

                    this.meterManager.Save();
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

            var encounter = this.GeneralConfigPage.Preview ? this.previewEvent : this.encounterManager.GetEncounter(this.eventIndex);

            (localPos, size) = this.HeaderConfigPage.DrawHeader(localPos, size, encounter, drawList);
            drawList.AddRectFilled(localPos, localPos + size, generalConfig.BackgroundColor.Base);
            this.DrawBars(drawList, localPos, size, encounter);
        });

        this.lastFrameWasUnlocked = this.unlocked;
        this.lastFrameWasPreview = this.GeneralConfigPage.Preview;
        this.lastFrameWasCombat = combat;
    }

    // Dont ask
    protected void UpdateDragData(Vector2 pos, Vector2 size, bool locked)
    {
        this.unlocked = !locked;
        this.hovered = ImGui.IsMouseHoveringRect(pos, pos + size);
        this.dragging = this.lastFrameWasDragging && ImGui.IsMouseDown(ImGuiMouseButton.Left);
        this.locked = ((this.unlocked && !this.lastFrameWasUnlocked) || !this.hovered) && !this.dragging;
        this.lastFrameWasDragging = this.hovered || this.dragging;
    }

    private void DrawBars(ImDrawListPtr drawList, Vector2 localPos, Vector2 size, Encounter? actEvent)
    {
        if (actEvent?.AllyCombatants is not null && actEvent.AllyCombatants.Any())
        {
            var dataType = this.Config.GeneralConfig.DataType;
            var sortedCombatants = this.GetSortedCombatants(actEvent, dataType);

            float top = dataType switch
                        {
                            MeterDataType.Damage => sortedCombatants[0].DamageTotal,
                            MeterDataType.Healing => sortedCombatants[0].HealingTotal,
                            MeterDataType.EffectiveHealing => sortedCombatants[0].EffectiveHealing,
                            MeterDataType.DamageTaken => sortedCombatants[0].DamageTaken,
                            _ => 0,
                        };

            var i = 0;
            var barConfig = this.Config.BarConfig;
            if (sortedCombatants.Count > barConfig.BarCount)
            {
                i = Math.Clamp(this.scrollPosition, 0, sortedCombatants.Count - barConfig.BarCount);
                this.scrollPosition = i;
            }

            var maxIndex = Math.Min(i + barConfig.BarCount, sortedCombatants.Count);
            for (; i < maxIndex; i++)
            {
                var combatant = sortedCombatants[i];
                combatant.Rank = (i + 1).ToString();

                float current = dataType switch
                                {
                                    MeterDataType.Damage => combatant.DamageTotal,
                                    MeterDataType.Healing => combatant.HealingTotal,
                                    MeterDataType.EffectiveHealing => combatant.EffectiveHealing,
                                    MeterDataType.DamageTaken => combatant.DamageTaken,
                                    _ => 0,
                                };

                var barColor = barConfig.BarColor;
                var jobColor = this.BarColorsConfigPage.GetColor(combatant.Job);
                localPos = this.BarConfigPage.DrawBar(drawList, localPos, size, combatant, jobColor, barColor, Math.Max(maxIndex, barConfig.MinBarCount), top, current);
            }
        }
    }

    private bool DrawContextMenu(string popupId, out int selectedIndex)
    {
        selectedIndex = -1;
        var selected = false;

        if (ImGui.BeginPopup(popupId))
        {
            if (!ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                ImGui.SetKeyboardFocusHere(0);
            }

            if (ImGui.Selectable("Current Data"))
            {
                selected = true;
            }

            var events = this.encounterManager.EncounterHistory;
            if (events.Count > 0)
            {
                ImGui.Separator();
            }

            for (var i = events.Count - 1; i >= 0; i--)
            {
                if (ImGui.Selectable($"{events[i].Start:T}\t—\t{events[i].Title}"))
                {
                    selectedIndex = i;
                    selected = true;
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

    private List<Combatant> GetSortedCombatants(Encounter actEvent, MeterDataType dataType)
    {
        if (this.lastSortedTimestamp.HasValue && this.lastSortedTimestamp.Value == actEvent.LastEvent &&
            !this.GeneralConfigPage.Preview)
        {
            return this.lastSortedCombatants;
        }

        var sortedCombatants = actEvent.AllyCombatants
           .Where(c => c.Kind != BattleNpcSubKind.Enemy)
           .ToList();

        sortedCombatants.Sort((x, y) =>
        {
            float xFloat = dataType switch
                           {
                               MeterDataType.Damage => x.DamageTotal,
                               MeterDataType.Healing => x.HealingTotal,
                               MeterDataType.EffectiveHealing => x.EffectiveHealing,
                               MeterDataType.DamageTaken => x.DamageTaken,
                               _ => 0,
                           };

            float yFloat = dataType switch
                           {
                               MeterDataType.Damage => y.DamageTotal,
                               MeterDataType.Healing => y.HealingTotal,
                               MeterDataType.EffectiveHealing => x.EffectiveHealing,
                               MeterDataType.DamageTaken => y.DamageTaken,
                               _ => 0,
                           };

            return (int)(yFloat - xFloat);
        });

        this.lastSortedTimestamp = actEvent.LastEvent;
        this.lastSortedCombatants = sortedCombatants;
        return sortedCombatants;
    }
}