// <copyright file="BuffInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using Lumina.Excel.GeneratedSheets;

internal class BuffInfo
{
    internal BuffInfo(Item item, uint itemFoodId, StatModifiers stats, bool hq)
    {
        this.Item = item;
        this.ItemFoodId = itemFoodId;
        this.Stats = stats;
        this.IsHq = hq;
    }

    internal Item Item { get; }

    internal uint ItemFoodId { get; }

    internal StatModifiers Stats { get; }

    internal bool IsHq { get; }
}