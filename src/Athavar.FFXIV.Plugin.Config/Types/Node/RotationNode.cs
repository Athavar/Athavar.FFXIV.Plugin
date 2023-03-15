// <copyright file="RotationNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

public sealed class RotationNode : INode
{
    /// <inheritdoc />
    public string Name { get; set; } = string.Empty;

    public int[] Rotations { get; set; } = { };

    /*[JsonIgnore]
    internal CraftingMacro? Macro { get; set; }

    internal CraftingMacro InitMarco() => this.Macro ??= CraftingSkill.Parse(this.Rotations);

    internal void Update(string text) => this.Macro = CraftingSkill.Parse(CraftingSkill.Parse(text));

    internal void Save()
    {
        if (this.Macro is null)
        {
            return;
        }

        this.Rotations = this.Macro.Rotation.Select(r => r.Skill).ToArray();
    }*/
}