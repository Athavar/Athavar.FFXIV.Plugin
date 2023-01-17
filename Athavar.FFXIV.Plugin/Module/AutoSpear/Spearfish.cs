// <copyright file="Spearfish.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using System;
using System.Collections.Generic;
using Athavar.FFXIV.Plugin.Module.AutoSpear.Enum;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Data;
using ItemRow = Lumina.Excel.GeneratedSheets.Item;
using SpearFishRow = Lumina.Excel.GeneratedSheets.SpearfishingItem;

internal class SpearFish : IComparable<SpearFish>
{
    private readonly object fishData;

    public SpearFish(DataManager gameData, SpearFishRow fishRow)
    {
        this.ItemData = fishRow.Item.Value ?? new ItemRow();
        this.fishData = fishRow;
        this.Name = MultiString.FromItem(gameData, this.ItemData.RowId);
        this.Size = SpearfishSize.Unknown;
        this.Speed = SpearfishSpeed.Unknown;
    }

    public SpearFishRow? SpearfishData => this.fishData as SpearFishRow;

    public uint ItemId => this.ItemData.RowId;

    public uint FishId => this.SpearfishData!.RowId;

    public SpearfishSize Size { get; internal set; } = SpearfishSize.Unknown;

    public SpearfishSpeed Speed { get; internal set; } = SpearfishSpeed.Unknown;

    public ItemRow ItemData { get; init; }

    public IList<FishingSpot> FishingSpots { get; init; } = new List<FishingSpot>();

    public MultiString Name { get; init; }

    public int CompareTo(SpearFish? obj) => this.ItemId.CompareTo(obj?.ItemId ?? 0);
}