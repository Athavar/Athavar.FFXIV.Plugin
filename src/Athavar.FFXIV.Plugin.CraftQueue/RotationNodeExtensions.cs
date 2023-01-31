// <copyright file="RotationNodeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.CraftSimulator.Models;

internal static class RotationNodeExtensions
{
    internal static void Save(this RotationNode node, CraftingMacro? macro)
    {
        if (macro is null)
        {
            return;
        }

        node.Rotations = macro.Rotation.Select(r => (int)r.Skill).ToArray();
    }
}