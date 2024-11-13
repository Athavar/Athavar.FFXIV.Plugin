// <copyright file="FishingSpot.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Lumina.Excel.Sheets;

internal sealed class FishingSpot
{
    public const uint SpearfishingIdOffset = 1u << 31;

    private readonly object data;

    public FishingSpot(IDictionary<uint, SpearFish> data, SpearfishingNotebook spot)
    {
        this.data = spot;

        this.Items = spot.GatheringPointBase.Value.Item.Where(i => i.RowId > 0)
                        .Select(i => data.Values.FirstOrDefault(f => f.FishId == i.RowId))
                        .Where(f => f != null).Cast<SpearFish>()
                        .ToArray()
                  ?? [];

        foreach (var item in this.Items)
        {
            item.FishingSpots.Add(this);
        }
    }

    public SpearfishingNotebook? SpearfishingSpotData => (SpearfishingNotebook)this.data;

    public Lumina.Excel.Sheets.FishingSpot? FishingSpotData => (Lumina.Excel.Sheets.FishingSpot)this.data;

    public uint SheetId
        => this.data is SpearfishingNotebook sf
            ? sf.RowId
            : ((Lumina.Excel.Sheets.FishingSpot)this.data).RowId;

    public uint Id
        => this.data is SpearfishingNotebook sf
            ? sf.RowId | SpearfishingIdOffset
            : ((Lumina.Excel.Sheets.FishingSpot)this.data).RowId;

    public bool Spearfishing => this.data is SpearfishingNotebook;

    public SpearFish[] Items { get; init; }

    public int CompareTo(FishingSpot? obj) => this.SheetId.CompareTo(obj?.SheetId ?? 0);

    private TerritoryType? FishingSpotTerritoryHacks(IDalamudServices data, Lumina.Excel.Sheets.FishingSpot spot)
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