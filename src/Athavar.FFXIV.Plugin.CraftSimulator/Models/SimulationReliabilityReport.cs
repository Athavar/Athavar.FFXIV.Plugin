// <copyright file="SimulationReliabilityReport.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public sealed record SimulationReliabilityReport(SimulationResult[] RawData, int SuccessPercent, int AverageHqPercent, int MedianHqPercent, int MinHqPercent, int MaxHqPercent)
{
}