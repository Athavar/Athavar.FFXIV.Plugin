// <copyright file="IRotationResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Interfaces;

using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;

internal interface IRotationResolver
{
    public ResolverType ResolverType { get; }

    public string Name { get; }

    public int Length { get; }

    public CraftingSkills? GetNextAction(int index);
}