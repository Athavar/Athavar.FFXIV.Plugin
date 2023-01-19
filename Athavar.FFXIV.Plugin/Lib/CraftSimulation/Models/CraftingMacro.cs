// <copyright file="CraftingMacro.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using System.Linq;
using Dalamud;

internal class CraftingMacro
{
    public CraftingSkill[] Rotation { get; init; }

    public string CreateMacro(ClientLanguage language)
    {
        string Process(CraftingSkill skill) => $"/ac \"{skill.Name[language]}\" <wait.{skill.Action.GetWaitDuration()}>";

        return string.Join("\r\n", this.Rotation.Select(Process));
    }

    public CraftingSkills[] ToEmum() => this.Rotation.Select(r => r.Skill).ToArray();
}