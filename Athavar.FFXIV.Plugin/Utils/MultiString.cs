// <copyright file="MultiString.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Utils;

using System;
using Dalamud;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;

public readonly struct MultiString
{
    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);

    public readonly string English;
    public readonly string German;
    public readonly string French;
    public readonly string Japanese;

    public MultiString(string en, string de, string fr, string jp)
    {
        this.English = en;
        this.German = de;
        this.French = fr;
        this.Japanese = jp;
    }

    public string this[ClientLanguage lang] => this.Name(lang);

    public static string ParseSeStringLumina(SeString? luminaString) => luminaString == null ? string.Empty : Dalamud.Game.Text.SeStringHandling.SeString.Parse(luminaString.RawData).TextValue;

    public static MultiString FromPlaceName(DataManager gameData, uint id)
    {
        var en = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.English)!.GetRow(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.German)!.GetRow(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.French)!.GetRow(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<PlaceName>(ClientLanguage.Japanese)!.GetRow(id)?.Name);
        return new MultiString(en, de, fr, jp);
    }

    public static MultiString FromItem(DataManager gameData, uint id)
    {
        var en = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.English)!.GetRow(id)?.Name);
        var de = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.German)!.GetRow(id)?.Name);
        var fr = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.French)!.GetRow(id)?.Name);
        var jp = ParseSeStringLumina(gameData.GetExcelSheet<Item>(ClientLanguage.Japanese)!.GetRow(id)?.Name);
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
}