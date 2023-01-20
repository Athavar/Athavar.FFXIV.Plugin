// <copyright file="CrafterStatsExtension.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Extension;

using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Game.ClientState.Objects.SubKinds;

internal static class CrafterStatsExtensions
{
    public static void Apply(this CrafterStats stats, PlayerCharacter character) => stats.Level = character.Level;

    public static void Apply(this CrafterStats stats, Gearset gearset)
    {
        stats.CP = gearset.CP;
        stats.Control = gearset.Control;
        stats.Craftsmanship = gearset.Craftsmanship;
    }
}