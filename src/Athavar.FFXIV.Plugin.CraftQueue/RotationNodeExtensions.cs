// <copyright file="RotationNodeExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using System.Collections.Immutable;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;

internal static class RotationNodeExtensions
{
    internal static void Save(this RotationNode node, CraftingMacro? macro)
    {
        if (macro is null)
        {
            return;
        }

        node.Rotations = macro.Rotation.Select(r => (int)r.Skill).ToImmutableArray();
    }
}