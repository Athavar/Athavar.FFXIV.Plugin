// <copyright file="RegexExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Extension;

using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

public static class RegexExtensions
{
    [Pure]
    public static string ReplaceGroupValue(this Regex source, string input, string groupName, string destinationValue)
        => ReplaceGroupValue(
            source,
            input,
            m => m.Groups[groupName],
            p => destinationValue);

    [Pure]
    public static string ReplaceGroupValue(this Regex source, string input, int groupIdx, string destinationValue)
        => ReplaceGroupValue(
            source,
            input,
            m => m.Groups[groupIdx],
            p => destinationValue);

    [Pure]
    public static string ReplaceGroupValue(this Regex source, string input, string groupName, Func<string, string> destinationValueSelector)
        => ReplaceGroupValue(
            source,
            input,
            m => m.Groups[groupName],
            destinationValueSelector);

    [Pure]
    public static string ReplaceGroupValue(this Regex source, string input, int groupIdx, Func<string, string> destinationValueSelector)
        => ReplaceGroupValue(
            source,
            input,
            m => m.Groups[groupIdx],
            destinationValueSelector);

    [Pure]
    private static string ReplaceGroupValue(
        Regex source,
        string input,
        Func<Match, Group> groupSelector,
        Func<string, string> destinationValueSelector)
    {
        var matchResult = source.Matches(input);

        if (matchResult.Count <= 0)
        {
            return input;
        }

        var text = input;

        foreach (var group in matchResult.Select(groupSelector).OrderByDescending(p => p.Index))
        {
            var begin = group.Index > 0 ? text.Substring(0, group.Index) : string.Empty;
            var end = group.Index + group.Length < text.Length
                ? text.Substring(group.Index + group.Length)
                : string.Empty;
            var destinationValue = destinationValueSelector.Invoke(group.Value);
            text = $"{begin}{destinationValue}{end}";
        }

        return text;
    }
}