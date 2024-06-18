// <copyright file="CraftingJob.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Job;

using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.Models;

internal class CraftingJob(CraftQueue queue, RecipeExtended recipe, IRotationResolver rotationResolver, Gearset gearset, uint count, BuffConfig buffConfig, (uint ItemId, byte Amount)[] hqIngredients, CraftingJobFlags flags)
    : BaseCraftingJob(queue, recipe, rotationResolver, gearset, count, buffConfig, hqIngredients, flags);