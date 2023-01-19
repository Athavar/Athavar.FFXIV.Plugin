// <copyright file="SimulationResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

using System.Collections.Generic;

internal class SimulationResult
{
    public List<ActionResult> Steps { get; init; }

    public int HqPercent { get; init; }

    public bool Success { get; set; }

    public Simulation Simulation { get; init; }

    public SimulationFailCause? FailCause { get; set; }
}