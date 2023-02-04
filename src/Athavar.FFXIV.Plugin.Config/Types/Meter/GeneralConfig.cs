// <copyright file="GeneralConfig.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;

public class GeneralConfig : BaseConfig
{
    [JsonIgnore]
    public bool Preview { get; set; } = false;

    public Vector2 Position { get; set; } = Vector2.Zero;

    public Vector2 Size { get; set; } = new((ImGui.GetMainViewport().Size.Y * 16) / 90, ImGui.GetMainViewport().Size.Y / 10);

    public bool Lock { get; set; }

    public bool ClickThrough { get; set; }

    public ConfigColor BackgroundColor { get; set; } = new(0, 0, 0, 0.5f);

    public bool ShowBorder { get; set; } = true;

    public bool BorderAroundBars { get; set; }

    public ConfigColor BorderColor { get; set; } = new(30f / 255f, 30f / 255f, 30f / 255f, 230f / 255f);

    public int BorderThickness { get; set; } = 2;

    public MeterDataType DataType { get; set; } = MeterDataType.Damage;

    public bool ReturnToCurrent { get; set; } = true;
}