// <copyright file="IBaseCraftingJob.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.Interfaces;

using Athavar.FFXIV.Plugin.CraftQueue.Job;

internal interface IBaseCraftingJob
{
    CraftingJobFlags Flags { get; }

    string RotationName { get; }

    RecipeExtended Recipe { get; }

    (uint ItemId, byte Amount)[] HqIngredients { get; }
}