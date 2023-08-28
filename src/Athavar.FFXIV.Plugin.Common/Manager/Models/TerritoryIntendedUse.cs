// <copyright file="TerritoryIntendedUse.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Models;

using Athavar.FFXIV.Plugin.Models;

public enum TerritoryIntendedUse : byte
{
    City = 0,
    Overworld1 = 1,
    InnRoom = 2,
    Dungeon = 3,
    VariantDungeon = 4,
    MordianGaol = 5,
    NewCharacterCity = 6,
    StoryAreas1 = 7,
    AllianceRaid = 8,
    Overworld2 = 9,
    Trial = 10,
    HousingWard = 13,
    HousingInstance = 14,
    InstancedOpenZone = 15, // Story?
    Raid1 = 16,
    Raid2 = 17,
    AlliancePvp = 18,
    ChocoboSquare1 = 19,
    ChocoboSquare2 = 20,
    TheFirmament = 21,
    SanctumOfTheTwelve = 22,
    LordOfVerminion = 25,
    TheDiadem1 = 26,
    HallOfTheNovice = 27,
    CrystallineConflict1 = 28,
    MsqSoloDuty = 29,
    GrandCompanyBarracks = 30,
    DeepDungeon = 31,
    HolidayInstance = 32,
    MapPortal = 33,
    HolidayDuty = 34,
    TripleTriadBattlehall = 35,
    CrystallineConflict2 = 37,
    TheDiademHuntingGrounds = 38,
    RivalWings = 39,
    StoryAreas3 = 40,
    Eureka = 41,
    TheCalamityRetold = 43,
    LeapOfFaith = 44,
    MaskedCarnivale = 45,
    OceanFishing = 46,
    TheDiadem2 = 47,
    Bozja = 48,
    IslandSanctum = 49,
    TripleTriadTournament = 50,
    TripleTriadParlor = 51,
    DelebrumReginae = 52,
    DelebrumReginaeSavage = 53,
    StoryAreas4 = 54,
    StoryAreas5 = 56,
    CriterionDungeon = 57,
    CriterionDungeonSavage = 58,
}

public static class TerritoryIntendedUseExtensions
{
    public static bool HasAlliance(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.AllianceRaid => true,
            TerritoryIntendedUse.AlliancePvp => true,
            TerritoryIntendedUse.RivalWings => true,
            _ => false,
        };

    public static bool UsesBothGroupManagers(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.RivalWings => true,
            TerritoryIntendedUse.DelebrumReginae => true,
            TerritoryIntendedUse.DelebrumReginaeSavage => true,
            _ => false,
        };

    public static bool IsRaid(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.Raid1 => true,
            TerritoryIntendedUse.Raid2 => true,
            _ => false,
        };

    public static bool IsTrial(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.Trial => true,
            _ => false,
        };

    public static bool IsRaidOrTrial(this TerritoryIntendedUse territory) => IsRaid(territory) || IsTrial(territory);

    public static bool IsDuty(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.Dungeon => true,
            TerritoryIntendedUse.VariantDungeon => true,
            TerritoryIntendedUse.AllianceRaid => true,
            TerritoryIntendedUse.Trial => true,
            TerritoryIntendedUse.Raid1 => true,
            TerritoryIntendedUse.Raid2 => true,
            TerritoryIntendedUse.AlliancePvp => true,
            TerritoryIntendedUse.LordOfVerminion => true,
            TerritoryIntendedUse.TheDiadem1 => true,
            TerritoryIntendedUse.CrystallineConflict1 => true,
            TerritoryIntendedUse.DeepDungeon => true,
            TerritoryIntendedUse.MapPortal => true,
            TerritoryIntendedUse.HolidayDuty => true,
            TerritoryIntendedUse.CrystallineConflict2 => true,
            TerritoryIntendedUse.TheDiademHuntingGrounds => true,
            TerritoryIntendedUse.RivalWings => true,
            TerritoryIntendedUse.Eureka => true,
            TerritoryIntendedUse.TheCalamityRetold => true,
            TerritoryIntendedUse.MaskedCarnivale => true,
            TerritoryIntendedUse.TheDiadem2 => true,
            TerritoryIntendedUse.Bozja => true,
            TerritoryIntendedUse.DelebrumReginae => true,
            TerritoryIntendedUse.DelebrumReginaeSavage => true,
            TerritoryIntendedUse.CriterionDungeon => true,
            TerritoryIntendedUse.CriterionDungeonSavage => true,
            _ => false,
        };

    public static AllianceType GetAllianceType(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.RivalWings => AllianceType.SixParty,
            TerritoryIntendedUse.AllianceRaid => AllianceType.ThreeParty,
            TerritoryIntendedUse.AlliancePvp => AllianceType.SixParty,
            _ => AllianceType.None,
        };
}