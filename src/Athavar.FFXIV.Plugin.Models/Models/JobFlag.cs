// <copyright file="JobFlag.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models;

[Flags]
public enum JobFlag : ulong
{
    Adventure = 1ul << 0,
    Gladiator = 1ul << 1,
    Pugilist = 1ul << 2,
    Marauder = 1ul << 3,
    Lancer = 1ul << 4,

    Archer = 1ul << 5,
    Conjurer = 1ul << 6,
    Thaumaturge = 1ul << 7,
    Carpenter = 1ul << 8,

    Blacksmith = 1ul << 9,
    Armorer = 1ul << 10,
    Goldsmith = 1ul << 11,
    Leatherworker = 1ul << 12,

    Weaver = 1ul << 13,
    Alchemist = 1ul << 14,
    Culinarian = 1ul << 15,
    Miner = 1ul << 16,

    Botanist = 1ul << 17,
    Fisher = 1ul << 18,
    Paladin = 1ul << 19,
    Monk = 1ul << 20,

    Warrior = 1ul << 21,
    Dragoon = 1ul << 22,
    Bard = 1ul << 23,
    WhiteMage = 1ul << 24,

    BlackMage = 1ul << 25,
    Arcanist = 1ul << 26,
    Summoner = 1ul << 27,
    Scholar = 1ul << 28,

    Rogue = 1ul << 29,
    Ninja = 1ul << 30,
    Machinist = 1ul << 31,
    DarkKnight = 1ul << 32,

    Astrologian = 1ul << 33,
    Samurai = 1ul << 34,
    RedMage = 1ul << 35,
    BlueMage = 1ul << 36,

    Gunbreaker = 1ul << 37,
    Dancer = 1ul << 38,
    Reaper = 1ul << 39,
    Sage = 1ul << 40,
    Viper = 1ul << 41,
    Pictomancer = 1ul << 42,
}