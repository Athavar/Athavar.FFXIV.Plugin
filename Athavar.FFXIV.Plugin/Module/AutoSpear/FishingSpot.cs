// <copyright file="FishingSpot.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.AutoSpear;

using System;
using System.Collections.Generic;
using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Lumina.Excel.GeneratedSheets;

internal class FishingSpot
{
    public const uint SpearfishingIdOffset = 1u << 31;

    private readonly object _data;

    public FishingSpot(IDictionary<uint, SpearFish> data, SpearfishingNotebook spot)
    {
        this._data = spot;

        this.Items = spot.GatheringPointBase.Value?.Item.Where(i => i > 0)
                        .Select(i => data.Values.FirstOrDefault(f => f.FishId == i))
                        .Where(f => f != null).Cast<SpearFish>()
                        .ToArray()
                  ?? Array.Empty<SpearFish>();

        foreach (var item in this.Items)
        {
            item.FishingSpots.Add(this);
        }
    }

    public SpearfishingNotebook? SpearfishingSpotData => this._data as SpearfishingNotebook;

    public Lumina.Excel.GeneratedSheets.FishingSpot? FishingSpotData => this._data as Lumina.Excel.GeneratedSheets.FishingSpot;

    public uint SheetId
        => this._data is SpearfishingNotebook sf
            ? sf.RowId
            : ((Lumina.Excel.GeneratedSheets.FishingSpot)this._data).RowId;

    public uint Id
        => this._data is SpearfishingNotebook sf
            ? sf.RowId | SpearfishingIdOffset
            : ((Lumina.Excel.GeneratedSheets.FishingSpot)this._data).RowId;

    public bool Spearfishing => this._data is SpearfishingNotebook;

    public SpearFish[] Items { get; init; }

    public int CompareTo(FishingSpot? obj) => this.SheetId.CompareTo(obj?.SheetId ?? 0);

    private TerritoryType? FishingSpotTerritoryHacks(IDalamudServices data, Lumina.Excel.GeneratedSheets.FishingSpot spot)
        => spot.RowId switch
           {
               10_000 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(759), // the rows in between are no longer used diadem objects
               10_017 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_018 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_019 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_020 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_021 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_022 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_023 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_024 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               10_025 => data.DataManager.GetExcelSheet<TerritoryType>()!.GetRow(939),
               _ => spot.TerritoryType.Value,
           };
}