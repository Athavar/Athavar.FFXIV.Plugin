// <copyright file="MultiString.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Utils;

using Dalamud.Game;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using SeString = Dalamud.Game.Text.SeStringHandling.SeString;

public record MultiString(string English, string German, string French, string Japanese) : IEquatable<string>
{
    public static readonly MultiString Empty = new(string.Empty, string.Empty, string.Empty, string.Empty);

    public string this[ClientLanguage lang] => this.Name(lang);

    /// <inheritdoc/>
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

    public static MultiString FromPlaceName(IDataManager gameData, uint id) => From<PlaceName>(gameData, id, item => item?.Name);

    public static MultiString FromItem(IDataManager gameData, uint id) => From<Item>(gameData, id, item => item?.Name);

    public static MultiString FromAction(IDataManager gameData, uint id) => From<Action>(gameData, id, item => item?.Name);

    public static MultiString FromCraftAction(IDataManager gameData, uint id) => From<CraftAction>(gameData, id, item => item?.Name);

    public override string ToString() => this.Name(ClientLanguage.English);

    public string ToWholeString() => $"{this.English}|{this.German}|{this.French}|{this.Japanese}";

    public override int GetHashCode() => HashCode.Combine(this.English, this.German, this.French, this.Japanese);

    private static MultiString From<T>(IDataManager gameData, uint id, Func<T?, ReadOnlySeString?> action)
        where T : struct, IExcelRow<T>
    {
        string ParseSeStringLumina(ReadOnlySeString? luminaString) => luminaString == null ? string.Empty : SeString.Parse(luminaString.GetValueOrDefault()).TextValue;

        var en = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.English)!.GetRow(id)));
        var de = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.German)!.GetRow(id)));
        var fr = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.French)!.GetRow(id)));
        var jp = ParseSeStringLumina(action(gameData.GetExcelSheet<T>(ClientLanguage.Japanese)!.GetRow(id)));
        return new MultiString(en, de, fr, jp);
    }

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