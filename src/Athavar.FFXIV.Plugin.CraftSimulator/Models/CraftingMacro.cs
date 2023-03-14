// <copyright file="CraftingMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

using Dalamud;

public sealed class CraftingMacro
{
    public CraftingMacro(CraftingSkill[] rotation) => this.Rotation = rotation;

    public CraftingSkill[] Rotation { get; init; }

    public string CreateMacro(ClientLanguage language)
    {
        string Process(CraftingSkill skill) => $"/ac \"{skill.Name[language]}\" <wait.{skill.Action.GetWaitDuration()}>";

        return string.Join("\r\n", this.Rotation.Select(Process));
    }

    public CraftingSkills[] ToEmum() => this.Rotation.Select(r => r.Skill).ToArray();
}