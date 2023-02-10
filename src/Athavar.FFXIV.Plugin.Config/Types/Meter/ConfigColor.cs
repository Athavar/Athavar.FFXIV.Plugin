// <copyright file="ConfigColor.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Config;

using System.Numerics;
using Newtonsoft.Json;

public class ConfigColor
{
    [JsonIgnore]
    private float[] colorMapRatios = { -.8f, -.3f, .1f };

    [JsonIgnore]
    private Vector4 vector;

    // Constructor for deserialization
    public ConfigColor()
        : this(Vector4.Zero)
    {
    }

    public ConfigColor(Vector4 vector, float[]? colorMapRatios = null)
    {
        if (colorMapRatios != null && colorMapRatios.Length == 3)
        {
            this.colorMapRatios = colorMapRatios;
        }

        this.Vector = vector;
    }

    public ConfigColor(float r, float g, float b, float a, float[]? colorMapRatios = null)
        : this(new Vector4(r, g, b, a), colorMapRatios)
    {
    }

    public Vector4 Vector
    {
        get => this.vector;
        set
        {
            if (this.vector == value)
            {
                return;
            }

            this.vector = value;

            this.Update();
        }
    }

    [JsonIgnore]
    public uint Base { get; private set; }

    [JsonIgnore]
    public uint Background { get; private set; }

    [JsonIgnore]
    public uint TopGradient { get; private set; }

    [JsonIgnore]
    public uint BottomGradient { get; private set; }

    private static uint ColorConvertFloat4ToU32(Vector4 data)
    {
        uint ToByte(float f) => (uint)(((f < 0.0f ? 0.0f : f > 1.0f ? 1.0f : f) * 255.0f) + 0.5f);

        var result = ToByte(data.X) << 0;
        result |= ToByte(data.Y) << 8;
        result |= ToByte(data.Z) << 16;
        result |= ToByte(data.W) << 24;
        return result;
    }

    private void Update() => this.Base = ColorConvertFloat4ToU32(this.vector);
    /* Background = ImGui.ColorConvertFloat4ToU32(vector.AdjustColor(colorMapRatios[0]));
        // TopGradient = ImGui.ColorConvertFloat4ToU32(vector.AdjustColor(colorMapRatios[1]));
        // BottomGradient = ImGui.ColorConvertFloat4ToU32(vector.AdjustColor(colorMapRatios[2]));*/
}