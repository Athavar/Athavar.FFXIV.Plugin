// <copyright file="ActionResult.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public record ActionResult(CraftingSkill Skill, bool? Success, long AddedQuality, int AddedProgression, int CpDifference, bool Skipped, int DurabilityDifference, StepState State, SimulationFailCause? FailCause, bool Combo);