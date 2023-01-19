// <copyright file="BuffInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using Lumina.Excel.GeneratedSheets;

internal class BuffInfo
{
    internal BuffInfo(Item item, uint itemFoodId, StatModifiers nq, StatModifiers hq)
    {
        this.Item = item;
        this.ItemFoodId = itemFoodId;
        this.Nq = nq;
        this.Hq = hq;
    }

    internal Item Item { get; }

    internal uint ItemFoodId { get; }

    internal StatModifiers Nq { get; }

    internal StatModifiers Hq { get; }
}