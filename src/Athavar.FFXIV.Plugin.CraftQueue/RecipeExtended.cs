// <copyright file="RecipeExtended.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;

public sealed class RecipeExtended : Recipe
{
    private readonly uint? itemReq;

    /// <summary>
    ///     Gets the required item for the craft.
    /// </summary>
    public uint? ItemReq
    {
        get => this.itemReq;
        init => this.itemReq = value == 0 ? null : value;
    }

    /// <summary>
    ///     Gets or sets the result item name of the recipe.
    /// </summary>
    public string? ResultItemName { get; set; } = null;

    /// <summary>
    ///     Gets or sets the reuqired item name of the recipe.
    /// </summary>
    public string? RequiredItemName { get; set; } = null;
}