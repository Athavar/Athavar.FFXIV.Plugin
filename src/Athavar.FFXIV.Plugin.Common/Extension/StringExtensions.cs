// <copyright file="StringExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Extension;

/// <summary>
///     Extension methods.
/// </summary>
public static class StringExtensions
{
    /// <inheritdoc cref="string.Join(char, string?[])" />
    public static string Join(this IEnumerable<string> values, char separator) => string.Join(separator, values);

    /// <inheritdoc cref="string.Join(string, string?[])" />
    public static string Join(this IEnumerable<string> values, string separator) => string.Join(separator, values);
}