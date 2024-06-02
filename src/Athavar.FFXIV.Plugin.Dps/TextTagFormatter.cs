// <copyright file="TextTagFormatter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps;

using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;

public sealed class TextTagFormatter
{
    private readonly string format;
    private readonly Dictionary<string, PropertyInfo> propertys;
    private readonly object source;

    public TextTagFormatter(
        object source,
        string format,
        Dictionary<string, PropertyInfo> propertys)
    {
        this.source = source;
        this.format = format;
        this.propertys = propertys;
    }

    public static Regex TextTagRegex { get; } = new(@"\[(\w*)(:k)?\.?(\d+)?\]", RegexOptions.Compiled);

    public string Evaluate(Match m)
    {
        if (m.Groups.Count != 4)
        {
            return m.Value;
        }

        var format = string.IsNullOrEmpty(m.Groups[3].Value)
            ? $"{this.format}0"
            : $"{this.format}{m.Groups[3].Value}";

        string? value = null;
        var key = m.Groups[1].Value;

        if (this.propertys.ContainsKey(key))
        {
            var propValue = this.propertys[m.Groups[1].Value].GetValue(this.source);

            if (propValue is null)
            {
                return string.Empty;
            }

            var debug = m.Groups[1].Value.Equals("totaldamage");

            if (propValue is float fValue)
            {
                var kilo = !string.IsNullOrEmpty(m.Groups[2].Value);
                value = kilo ? KiloFormat(fValue, format) : fValue.ToString(format, CultureInfo.InvariantCulture);
            }
            else if (propValue is double dValue)
            {
                var kilo = !string.IsNullOrEmpty(m.Groups[2].Value);
                value = kilo ? KiloFormat(dValue, format) : dValue.ToString(format, CultureInfo.InvariantCulture);
            }
            else if (propValue is int iValue)
            {
                var kilo = !string.IsNullOrEmpty(m.Groups[2].Value);
                value = kilo ? KiloFormat(iValue, format) : iValue.ToString(format, CultureInfo.InvariantCulture);
            }
            else if (propValue is ulong ulValue)
            {
                var kilo = !string.IsNullOrEmpty(m.Groups[2].Value);
                value = kilo ? KiloFormat(ulValue, format) : ulValue.ToString(format, CultureInfo.InvariantCulture);
            }
            else
            {
                value = propValue.ToString();
                if (!string.IsNullOrEmpty(value) &&
                    int.TryParse(m.Groups[3].Value, out var trim) &&
                    trim < value.Length)
                {
                    value = propValue.ToString().AsSpan(0, trim).ToString();
                }
            }
        }

        return value ?? m.Value;
    }

    private static string KiloFormat(float num, string format)
        => num switch
        {
            >= 1000000 => (num / 1000000f).ToString(format, CultureInfo.InvariantCulture) + "M",
            >= 1000 => (num / 1000f).ToString(format, CultureInfo.InvariantCulture) + "K",
            _ => num.ToString(format, CultureInfo.InvariantCulture),
        };

    private static string KiloFormat(double num, string format)
        => num switch
        {
            >= 1000000 => (num / 1000000f).ToString(format, CultureInfo.InvariantCulture) + "M",
            >= 1000 => (num / 1000f).ToString(format, CultureInfo.InvariantCulture) + "K",
            _ => num.ToString(format, CultureInfo.InvariantCulture),
        };

    private static string KiloFormat(int num, string format)
        => num switch
        {
            >= 1000000 => (num / 1000000f).ToString(format, CultureInfo.InvariantCulture) + "M",
            >= 1000 => (num / 1000f).ToString(format, CultureInfo.InvariantCulture) + "K",
            _ => num.ToString(format, CultureInfo.InvariantCulture),
        };

    private static string KiloFormat(ulong num, string format)
        => num switch
        {
            >= 1000000 => (num / 1000000f).ToString(format, CultureInfo.InvariantCulture) + "M",
            >= 1000 => (num / 1000f).ToString(format, CultureInfo.InvariantCulture) + "K",
            _ => num.ToString(format, CultureInfo.InvariantCulture),
        };
}