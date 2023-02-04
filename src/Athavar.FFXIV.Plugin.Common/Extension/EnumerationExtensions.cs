// <copyright file="EnumerationExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

public static class EnumerationExtensions
{
    public static string AsText<T>(this T value)
        where T : Enum
        => Enum.GetName(typeof(T), value) ?? value.ToString();
}