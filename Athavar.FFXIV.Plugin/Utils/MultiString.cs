// <copyright file="MultiString.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Utils;

using System;
using Dalamud;
using Dalamud.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using Action = Lumina.Excel.GeneratedSheets.Action;

public record MultiString(string English, string German, string French, string Japanese) : IEquatable<string>
{
    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);

    public string this[ClientLanguage lang] => this.Name(lang);

    /// <inheritdoc />
    public virtual bool Equals(string? other)
    {
        if (this.English.Equals(other, StringComparison.InvariantCultureIgnoreCase) ||
            this.German.Equals(other, StringComparison.InvariantCultureIgnoreCase) ||
            this.French.Equals(other, StringComparison.InvariantCultureIgnoreCase) ||
            this.Japanese.Equals(other, StringComparison.InvariantCultureIgnoreCase))
        {
            return true;
        }

        return false;
    }

    public static MultiString FromPlaceName(DataManager gameData, uint id) => From<PlaceName>(gameData, id, item => item?.Name);

    public static MultiString FromItem(DataManager gameData, uint id) => From<Item>(gameData, id, item => item?.Name);

    public static MultiString FromAction(DataManager gameData, uint id) => From<Action>(gameData, id, item => item?.Name);

    public static MultiString FromCraftAction(DataManager gameData, uint id) => From<CraftAction>(gameData, id, item => item?.Name);

    private static MultiString From<T>(DataManager gameData, uint id, Func<T?, SeString?> action)
        where T : ExcelRow
    {
        string ParseSeStringLumina(SeString? luminaString) => luminaString == null ? string.Empty : Dalamud.Game.Text.SeStringHandling.SeString.Parse(luminaString.RawData).TextValue;

        var en = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.English)!.GetRow(id)));
        var de = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.German)!.GetRow(id)));
        var fr = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.French)!.GetRow(id)));
        var jp = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.Japanese)!.GetRow(id)));
        return new MultiString(en, de, fr, jp);
    }

    public override string ToString() => this.Name(ClientLanguage.English);

    public string ToWholeString() => $"{this.English}|{this.German}|{this.French}|{this.Japanese}";

    private string Name(ClientLanguage lang)
        => lang switch
           {
               ClientLanguage.English => this.English,
               ClientLanguage.German => this.German,
               ClientLanguage.Japanese => this.Japanese,
               ClientLanguage.French => this.French,
               _ => throw new ArgumentException(),
           };

    public override int GetHashCode() => HashCode.Combine(this.English, this.German, this.French, this.Japanese);
}