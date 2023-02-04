﻿namespace Athavar.FFXIV.Plugin.OpcodeWizard;

internal static class Util
{
    public static string NumberToString(int input, NumberDisplayFormat format)
    {
        var formatString = format switch
                           {
                               NumberDisplayFormat.Decimal => "",
                               NumberDisplayFormat.HexadecimalUppercase => "X4",
                               NumberDisplayFormat.HexadecimalLowercase => "x4",
                               _ => throw new NotImplementedException(),
                           };

        return !string.IsNullOrEmpty(formatString) ? $"0x{input.ToString(formatString)}" : input.ToString();
    }
}