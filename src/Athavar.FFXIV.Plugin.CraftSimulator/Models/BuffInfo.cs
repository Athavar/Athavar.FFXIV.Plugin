// <copyright file="BuffInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

using Lumina.Excel.GeneratedSheets;

public class BuffInfo
{
    public BuffInfo(Item item, uint itemFoodId, StatModifiers stats, bool hq)
    {
        this.Item = item;
        this.ItemFoodId = itemFoodId;
        this.Stats = stats;
        this.IsHq = hq;
    }

    public Item Item { get; }

    public uint ItemFoodId { get; }

    public StatModifiers Stats { get; }

    public bool IsHq { get; }
}