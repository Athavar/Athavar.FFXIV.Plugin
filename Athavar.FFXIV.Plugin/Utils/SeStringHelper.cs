// <copyright file="SeStringHelper.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Utils;

using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

/// <summary>
///     Methods to work with <see cref="SeString" />.
/// </summary>
public static class SeStringHelper
{
    private static readonly Regex TextPattern = new("(?<Parameter>{\\w+})|(?<AutoTranslate>{t:\\d+:\\d+})", RegexOptions.Compiled);

    /// <summary>
    ///     Parse a <see cref="string" /> to <see cref="SeString" />.
    ///     Allows Named Parameter replacement.
    /// </summary>
    /// <param name="text">The text that can contain placeholder.</param>
    /// <param name="parameter">Parameter for replacement.</param>
    /// <returns>returns the result <see cref="SeString" />.</returns>
    public static SeString Parse(string text, IDictionary<string, object>? parameter = null)
    {
        var seString = new SeString();
        var sparTextBuilder = new StringBuilder();

        void AppendPayload(Payload? payload)
        {
            string tmpText;
            if (!string.IsNullOrWhiteSpace(tmpText = sparTextBuilder.ToString()))
            {
                seString.Append(new TextPayload(tmpText));
                sparTextBuilder.Clear();
            }

            if (payload is not null)
            {
                seString.Append(payload);
            }
        }

        var parameterMatch = TextPattern.Matches(text);
        var lastIndex = 0;
        foreach (Match match in parameterMatch)
        {
            sparTextBuilder.Append(text.Substring(lastIndex, match.Index - lastIndex));
            lastIndex += (match.Length + match.Index) - lastIndex;

            if (match.Groups["Parameter"].Success && parameter is not null && parameter.TryGetValue(match.Value.Trim('{', '}').ToLowerInvariant(), out var value))
            {
                if (value is Payload payload)
                {
                    AppendPayload(payload);
                }
                else
                {
                    sparTextBuilder.Append(value);
                }
            }
            else if (match.Groups["AutoTranslate"].Success)
            {
                var parts = match.Value.Trim('{', '}').Split(':');
                if (parts.Length == 3 && uint.TryParse(parts[1], out var groupId) && uint.TryParse(parts[2], out var keyId))
                {
                    AppendPayload(new AutoTranslatePayload(groupId, keyId));
                }
                else
                {
                    sparTextBuilder.Append(match.Value);
                }
            }
            else
            {
                sparTextBuilder.Append(match.Value);
            }
        }

        sparTextBuilder.Append(text.Substring(lastIndex));
        AppendPayload(null);

        return seString;
    }

    /// <summary>Appends a single string to this <see cref="SeString" />.</summary>
    /// <param name="seString">The <see cref="SeString" /> to be append.</param>
    /// <param name="append">The string to append.</param>
    /// <returns>This object.</returns>
    public static SeString Append(this SeString seString, string append)
    {
        if (!string.IsNullOrWhiteSpace(append))
        {
            seString.Append(new TextPayload(append));
        }

        return seString;
    }
}