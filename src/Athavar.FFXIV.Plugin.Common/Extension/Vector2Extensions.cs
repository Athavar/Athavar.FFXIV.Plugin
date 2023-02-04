// <copyright file="Vector2Extensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

using System.Numerics;

public static class Vector2Extensions
{
    public static Vector2 AddX(this Vector2 v, float offset) => new(v.X + offset, v.Y);

    public static Vector2 AddY(this Vector2 v, float offset) => new(v.X, v.Y + offset);
}