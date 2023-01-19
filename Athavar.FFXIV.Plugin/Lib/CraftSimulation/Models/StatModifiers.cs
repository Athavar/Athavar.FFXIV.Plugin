// <copyright file="StatModifiers.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

internal class StatModifiers
{
    public int CraftsmanshipPct { get; set; }

    public int ControlPct { get; set; }

    public int CpPct { get; set; }

    public int CraftsmanshipMax { get; set; }

    public int ControlMax { get; set; }

    public int CpMax { get; set; }

    public bool Valid()
    {
        if ((this.CraftsmanshipPct > 0 && this.CraftsmanshipMax > 0) || (this.ControlPct > 0 && this.ControlMax > 0))
        {
            return true;
        }

        return this.CpPct > 0 && this.CpMax > 0;
    }
}