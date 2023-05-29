// <copyright file="CraftingMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

public sealed class CraftingMacro
{
    public CraftingMacro(CraftingSkill[] rotation) => this.Rotation = rotation;

    public CraftingSkill[] Rotation { get; init; }

    public CraftingSkills[] ToEnum() => this.Rotation.Select(r => r.Skill).ToArray();
}