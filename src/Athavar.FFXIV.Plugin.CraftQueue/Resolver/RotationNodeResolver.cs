// <copyright file="RotationNodeResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Resolver;

using Athavar.FFXIV.Plugin.CraftQueue.Interfaces;
using Athavar.FFXIV.Plugin.CraftQueue.Job;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;

/// <summary>
///     <see cref="BaseCraftingJob"/> resolver for <see cref="RotationNode"/>.
/// </summary>
/// <param name="node">The <see cref="RotationNode"/>.</param>
internal sealed class RotationNodeResolver(RotationNode node) : IStaticRotationResolver
{
    public CraftingSkills[] Rotation { get; } = CraftingSkill.Parse(node.Rotations).ToArray();

    public ResolverType ResolverType => ResolverType.Static;

    public string Name { get; } = node.Name;

    public int Length => this.Rotation.Length;

    public CraftingSkills? GetNextAction(int index) => this.Rotation.ElementAtOrDefault(index);
}