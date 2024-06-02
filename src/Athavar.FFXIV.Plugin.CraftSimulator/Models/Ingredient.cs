// <copyright file="Ingredient.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public sealed class Ingredient
{
    public Ingredient(uint id, ushort iconId, uint level, byte amount)
    {
        this.Id = id;
        this.IconId = iconId;
        this.ILevel = level;
        this.Amount = amount;
    }

    public uint Id { get; init; }

    public ushort IconId { get; init; }

    public uint ILevel { get; init; }

    public byte Amount { get; init; }

    public bool CanBeHq { get; set; } = false;

    public uint? Quality { get; set; }
}