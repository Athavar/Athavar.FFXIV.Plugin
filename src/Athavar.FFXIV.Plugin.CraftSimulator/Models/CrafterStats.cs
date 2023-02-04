// <copyright file="CrafterStats.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public class CrafterStats
{
    public CrafterStats(CrafterStats clone)
    {
        this.Level = clone.Level;
        this.Control = clone.Control;
        this.Craftsmanship = clone.Craftsmanship;
        this.CP = clone.CP;
        this.Specialist = clone.Specialist;
    }

    public CrafterStats(int level, uint control, uint craftsmanship, uint cp, bool specialist)
    {
        this.Level = level;
        this.Control = control;
        this.Craftsmanship = craftsmanship;
        this.CP = cp;
        this.Specialist = specialist;
    }

    public int Level { get; set; }

    public uint CP { get; set; }

    public uint Craftsmanship { get; set; }

    public uint Control { get; set; }

    public bool Specialist { get; set; }

    public void Apply(params StatModifiers?[] modifiers)
    {
        var baseCp = this.CP;
        var baseCraftsmanship = this.Craftsmanship;
        var baseControl = this.Control;

        foreach (var modifier in modifiers)
        {
            if (modifier is null)
            {
                continue;
            }

            this.CP += (uint)Math.Min(modifier.CpMax, (modifier.CpPct * baseCp) / 100);
            this.Craftsmanship += (uint)Math.Min(modifier.CraftsmanshipMax, (modifier.CraftsmanshipPct * baseCraftsmanship) / 100);
            this.Control += (uint)Math.Min(modifier.ControlMax, (modifier.ControlPct * baseControl) / 100);
        }
    }
}