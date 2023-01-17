// <copyright file="ActionResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;

internal record ActionResult(CraftingAction Action, bool? Success, long AddedQuality, int AddedProgression, int CpDifference, bool Skipped, int DurabilityDifference, StepState State, SimulationFailCause? FailCause, bool Combo);