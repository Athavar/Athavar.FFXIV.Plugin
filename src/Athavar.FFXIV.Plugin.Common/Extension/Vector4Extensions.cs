// <copyright file="Vector4Extensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

using System.Numerics;

public static class Vector4Extensions
{
    public static Vector4 AddTransparency(this Vector4 vec, float opacity) => new(vec.X, vec.Y, vec.Z, vec.W * opacity);

    public static Vector4 AdjustColor(this Vector4 vec, float correctionFactor)
    {
        var red = vec.X;
        var green = vec.Y;
        var blue = vec.Z;

        if (correctionFactor < 0)
        {
            correctionFactor = 1 + correctionFactor;
            red *= correctionFactor;
            green *= correctionFactor;
            blue *= correctionFactor;
        }
        else
        {
            red = ((1 - red) * correctionFactor) + red;
            green = ((1 - green) * correctionFactor) + green;
            blue = ((1 - blue) * correctionFactor) + blue;
        }

        return new Vector4(red, green, blue, vec.W);
    }
}