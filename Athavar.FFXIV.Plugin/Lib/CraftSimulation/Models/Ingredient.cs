// <copyright file="Ingredient.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using Lumina.Excel.GeneratedSheets;

internal class Ingredient
{
    public Ingredient(uint id, Item item, uint level, byte amount)
    {
        this.Id = id;
        this.Item = item;
        this.ILevel = level;
        this.Amount = amount;
    }

    public uint Id { get; init; }

    public Item Item { get; init; }

    public uint ILevel { get; init; }

    public byte Amount { get; init; }

    public bool CanBeHq { get; set; } = false;

    public uint? Quality { get; set; }
}