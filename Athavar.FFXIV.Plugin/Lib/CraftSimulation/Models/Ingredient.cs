// <copyright file="Ingredient.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

internal class Ingredient
{
    public uint Id { get; init; }

    public uint ILevel { get; init; }

    public byte Amount { get; init; }

    public bool CanBeHq { get; set; }

    public uint? Quality { get; set; }
}