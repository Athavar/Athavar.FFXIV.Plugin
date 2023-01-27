// <copyright file="MatchExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Extension;

using System.Text.RegularExpressions;

internal static class MatchExtensions
{
    /// <summary>
    ///     Extract a match group and unquote if necessary.
    /// </summary>
    /// <param name="match">Match group.</param>
    /// <param name="groupName">Group name.</param>
    /// <returns>Extracted and unquoted group value.</returns>
    public static string ExtractAndUnquote(this Match match, string groupName)
    {
        var group = match.Groups[groupName];
        var groupValue = group.Value;

        if (groupValue.StartsWith('"') && groupValue.EndsWith('"'))
        {
            groupValue = groupValue.Trim('"');
        }

        return groupValue;
    }
}