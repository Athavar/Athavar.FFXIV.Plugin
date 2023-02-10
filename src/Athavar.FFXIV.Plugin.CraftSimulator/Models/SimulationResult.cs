// <copyright file="SimulationResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public record SimulationResult(List<ActionResult> Steps, int HqPercent, Simulation Simulation)
{
    public bool Success { get; set; }

    public SimulationFailCause? FailCause { get; set; }
}