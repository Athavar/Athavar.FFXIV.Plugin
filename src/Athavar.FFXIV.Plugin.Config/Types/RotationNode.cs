// <copyright file="CraftMacroNode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

public class RotationNode : INode
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