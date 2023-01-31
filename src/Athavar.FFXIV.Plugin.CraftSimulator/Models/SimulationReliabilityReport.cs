// <copyright file="SimulationReliabilityReport.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public record SimulationReliabilityReport(SimulationResult[] RawData, int SuccessPercent, int AverageHqPercent, int MedianHqPercent, int MinHqPercent, int MaxHqPercent)
{
}