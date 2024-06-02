// <copyright file="EnumerationExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

public static class EnumerationExtensions
{
    public static string AsText<T>(this T value)
        where T : Enum
        => Enum.GetName(typeof(T), value) ?? value.ToString();
}