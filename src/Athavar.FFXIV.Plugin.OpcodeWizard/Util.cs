// <copyright file="Util.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard;

internal static class Util
{
    public static string NumberToString(int input, NumberDisplayFormat format)
    {
        var formatString = format switch
        {
            NumberDisplayFormat.Decimal => string.Empty,
            NumberDisplayFormat.HexadecimalUppercase => "X4",
            NumberDisplayFormat.HexadecimalLowercase => "x4",
            _ => throw new ArgumentOutOfRangeException(),
        };

        return !string.IsNullOrEmpty(formatString) ? $"0x{input.ToString(formatString)}" : input.ToString();
    }
}