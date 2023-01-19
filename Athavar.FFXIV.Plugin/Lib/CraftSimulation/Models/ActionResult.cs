// <copyright file="ActionResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

internal record ActionResult(CraftingSkill Skill, bool? Success, long AddedQuality, int AddedProgression, int CpDifference, bool Skipped, int DurabilityDifference, StepState State, SimulationFailCause? FailCause, bool Combo);