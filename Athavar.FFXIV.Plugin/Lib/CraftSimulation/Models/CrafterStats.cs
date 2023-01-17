// <copyright file="Crafter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

internal class CrafterStats
{
    public CrafterJobStats?[] Jobs { get; } = new CrafterJobStats?[8];
}

internal class CrafterJobStats
{
    public int Level { get; set; }

    public int CP { get; set; }

    public int Craftsmanship { get; set; }

    public int Control { get; set; }

    public bool Specialist { get; set; }
}