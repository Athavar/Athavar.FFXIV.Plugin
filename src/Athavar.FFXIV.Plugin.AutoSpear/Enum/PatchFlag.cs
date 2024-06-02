// <copyright file="PatchFlag.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.AutoSpear.Enum;

[Flags]
internal enum PatchFlag : ulong
{
    ARealmReborn = 1 << 0,
    ARealmAwoken = 1 << 1,
    ThroughTheMaelstrom = 1 << 2,
    DefendersOfEorzea = 1 << 3,
    DreamsOfIce = 1 << 4,
    BeforeTheFall = 1 << 5,
    Heavensward = 1 << 6,
    AsGoesLightSoGoesDarkness = 1 << 7,
    TheGearsOfChange = 1 << 8,
    RevengeOfTheHorde = 1 << 9,
    SoulSurrender = 1 << 10,
    TheFarEdgeOfFate = 1 << 11,
    Stormblood = 1 << 12,
    TheLegendReturns = 1 << 13,
    RiseOfANewSun = 1 << 14,
    UnderTheMoonlight = 1 << 15,
    PreludeInViolet = 1 << 16,
    ARequiemForHeroes = 1 << 17,
    Shadowbringers = 1 << 18,
    VowsOfVirtueDeedsOfCruelty = 1 << 19,
    EchoesOfAFallenStar = 1 << 20,
    ReflectionsInCrystal = 1 << 21,
    FuturesRewritten = 1 << 22,
    DeathUntoDawn = 1 << 23,
    Endwalker = 1 << 24,
    NewfoundAdventure = 1 << 25,
    BuriedMemory = 1 << 26,
    GodsRevelLandsTremble = 27,
}