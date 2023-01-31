// <copyright file="StringExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
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