// <copyright file="GeneralConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using ImGuiNET;
using Newtonsoft.Json;

public sealed class GeneralConfig : BaseConfig
{
    [JsonIgnore]
    public bool Preview { get; set; } = false;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public Vector2 Size { get; set; } = new((ImGui.GetMainViewport().Size.Y * 16) / 90, ImGui.GetMainViewport().Size.Y / 10);

    public bool Enabled { get; set; } = true;

    public bool Lock { get; set; }

    public bool ClickThrough { get; set; }

    public ConfigColor BackgroundColor { get; set; } = new(0, 0, 0, 0.5f);

    public bool ShowBorder { get; set; } = true;

    public bool BorderAroundBars { get; set; }

    public ConfigColor BorderColor { get; set; } = new(30f / 255f, 30f / 255f, 30f / 255f, 230f / 255f);

    public int BorderThickness { get; set; } = 2;

    public MeterDataType DataType { get; set; } = MeterDataType.Damage;

    public bool ShowActionSummaryTooltip { get; set; } = true;

    public VirtualKey ShowActionSummaryModifyKey { get; set; } = VirtualKey.NO_KEY;

    public bool ReturnToCurrent { get; set; } = true;
}