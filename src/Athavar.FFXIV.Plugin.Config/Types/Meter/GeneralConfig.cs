// <copyright file="GeneralConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;
using System.Text.Json.Serialization;
using Dalamud.Game.ClientState.Keys;
using ImGuiNET;

public sealed class GeneralConfig : BaseConfig
{
    [JsonIgnore]
    [Newtonsoft.Json.JsonIgnore]
    public bool Preview { get; set; } = false;

    [JsonInclude]
    [JsonPropertyName("Position")]
    public Vector2 Position { get; set; } = Vector2.Zero;

    [JsonInclude]
    [JsonPropertyName("Size")]
    public Vector2 Size { get; set; } = new((ImGui.GetMainViewport().Size.Y * 16) / 90, ImGui.GetMainViewport().Size.Y / 10);

    [JsonInclude]
    [JsonPropertyName("Enabled")]
    public bool Enabled { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("Lock")]
    public bool Lock { get; set; }

    [JsonInclude]
    [JsonPropertyName("ClickThrough")]
    public bool ClickThrough { get; set; }

    [JsonInclude]
    [JsonPropertyName("BackgroundColor")]
    public ConfigColor BackgroundColor { get; set; } = new(0, 0, 0, 0.5f);

    [JsonInclude]
    [JsonPropertyName("ShowBorder")]
    public bool ShowBorder { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("BorderAroundBars")]
    public bool BorderAroundBars { get; set; }

    [JsonInclude]
    [JsonPropertyName("BorderColor")]
    public ConfigColor BorderColor { get; set; } = new(30f / 255f, 30f / 255f, 30f / 255f, 230f / 255f);

    [JsonInclude]
    [JsonPropertyName("BorderThickness")]
    public int BorderThickness { get; set; } = 2;

    [JsonInclude]
    [JsonPropertyName("DataType")]
    public MeterDataType DataType { get; set; } = MeterDataType.Damage;

    [JsonInclude]
    [JsonPropertyName("ShowActionSummaryTooltip")]
    public bool ShowActionSummaryTooltip { get; set; } = true;

    [JsonInclude]
    [JsonPropertyName("ShowActionSummaryModifyKey")]
    public VirtualKey ShowActionSummaryModifyKey { get; set; } = VirtualKey.NO_KEY;

    [JsonInclude]
    [JsonPropertyName("ReturnToCurrent")]
    public bool ReturnToCurrent { get; set; } = true;
}