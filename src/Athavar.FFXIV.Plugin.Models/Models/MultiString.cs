// <copyright file="MultiString.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models;

using System.Text.Json.Serialization;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Text.ReadOnly;

public record MultiString(
    [property: JsonPropertyName("E")]
    string English,
    [property: JsonPropertyName("G")]
    string? German,
    [property: JsonPropertyName("F")]
    string? French,
    [property: JsonPropertyName("J")]
    string? Japanese) : IEquatable<string>
{
    public static readonly MultiString Empty = new(string.Empty, null, null, null);

    public string this[ClientLanguage lang] => this.Name(lang);

    /// <inheritdoc/>
    public virtual bool Equals(string? other)
    {
        if (string.Equals(this.English, other, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(this.German, other, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(this.French, other, StringComparison.InvariantCultureIgnoreCase) ||
            string.Equals(this.Japanese, other, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static MultiString From<T>(IDataManager gameData, uint id, Func<T?, ReadOnlySeString?> action)
        where T : struct, IExcelRow<T>
    {
        string ParseSeStringLumina(ReadOnlySeString? luminaString) => luminaString == null ? string.Empty : SeString.Parse(luminaString.GetValueOrDefault()).TextValue;

        var en = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.English)!.GetRow(id)));
        var de = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.German)!.GetRow(id)));
        var fr = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.French)!.GetRow(id)));
        var jp = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.Japanese)!.GetRow(id)));
        return new MultiString(en, de, fr, jp);
    }

    public override string ToString() => this.Name(ClientLanguage.English);

    public string ToWholeString() => $"{this.English}|{this.German}|{this.French}|{this.Japanese}";

    public override int GetHashCode() => HashCode.Combine(this.English, this.German, this.French, this.Japanese);

    private string Name(ClientLanguage lang)
        => lang switch
        {
            ClientLanguage.English => this.English,
            ClientLanguage.German => this.German ?? this.English,
            ClientLanguage.Japanese => this.Japanese ?? this.English,
            ClientLanguage.French => this.French ?? this.English,
            _ => throw new ArgumentException(),
        };
}