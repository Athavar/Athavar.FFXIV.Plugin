// <copyright file="BuffInfo.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public sealed class BuffInfo
{
    public BuffInfo(uint itemId, uint itemLevel, ushort iconId, string name, uint itemFoodId, StatModifiers stats, bool hq)
    {
        this.ItemId = itemId;
        this.ItemLevel = itemLevel;
        this.IconId = iconId;
        this.Name = name;
        this.ItemFoodId = itemFoodId;
        this.Stats = stats;
        this.IsHq = hq;
    }

    public uint ItemId { get; }

    public uint ItemLevel { get; }

    public ushort IconId { get; }

    public string Name { get; }

    public uint ItemFoodId { get; }

    public StatModifiers Stats { get; }

    public bool IsHq { get; }
}