// <copyright file="TerritoryIntendedUse.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Duty;

/// <summary>
///     For the column in the TerritoryType sheet.
///     Work done by Xpahtalo.
///     https://github.com/Xpahtalo/XpahtaLib/blob/febf944683111082fd0614e528dbe42b78a2dfdb/XpahtaLib/DalamudUtilities/UsefulEnums/TerritoryIntendedUseEnum.cs
/// </summary>
public enum TerritoryIntendedUse : byte
{
    /// <summary>
    ///     These are the main cities for expansions. This includes both zones if there are two for the city.
    /// </summary>
    /// <remarks>
    ///     This also includes The Doman Enclave, but not Gangos where you do relic stuff.
    /// </remarks>
    MainCity = 0,

    /// <summary>
    ///     Standard overworld zones.
    /// </summary>
    SharedOverworld = 1,

    /// <summary>
    ///     Inn room in the major cities.
    /// </summary>
    InnRoom = 2,

    /// <summary>
    ///     Zones that are used for Duties that fall under the Dungeons and Guildhests Duty Finder tabs.
    /// </summary>
    DungeonsAndGuildhests = 3,

    /// <summary>
    ///     The Variant Dungeons. Criterion versions fall under a different category.
    /// </summary>
    VariantDungeon = 4,

    /// <summary>
    ///     GM Jail
    /// </summary>
    MordionGaol = 5,

    /// <summary>
    ///     The zone that a brand new player starts in before it is in a shared space with others.
    /// </summary>
    NewCharacterCity = 6,

    /// <summary>
    ///     Zones that are shared among players that's primary purpose is to host a duty entrance.
    /// </summary>
    SharedWaitingRoom = 7,

    /// <summary>
    ///     The 24-player alliance raids.
    /// </summary>
    AllianceRaid = 8,

    /// <summary>
    ///     Quest Battles zones in the overworld prior to Endwalker.
    /// </summary>
    PreEwOverworldQuestBattle = 9,

    /// <summary>
    ///     Zones that are used for Duties that fall under the Trials Duty Finder tab.
    /// </summary>
    Trial = 10,

    /// <summary>
    ///     No <see cref="Lumina.Excel.GeneratedSheets2.TerritoryType"/> uses this currently.
    /// </summary>
    /// <remarks>
    ///     No history for this value being used.
    /// </remarks>
    CurrentlyUnused1 = 11,

    /// <summary>
    ///     Zones that a player is placed into after that have completed certain duties.
    /// </summary>
    PostDutyRoom = 12,

    /// <summary>
    ///     The outdoors area of the housing wards.rr
    /// </summary>
    HousingWard = 13,

    /// <summary>
    ///     Inside a house.
    /// </summary>
    /// <remarks>
    ///     Also includes the apartment lobby.
    /// </remarks>
    HousingInstance = 14,

    /// <summary>
    ///     These exist in the game's overworld, but are solo only. Typically for story.
    /// </summary>
    SoloOverworldInstances = 15,

    /// <summary>
    ///     Various Coils and Alexander raids.
    /// </summary>
    Raid1 = 16,

    /// <summary>
    ///     All raids excluding the ones in <see cref="Raid1"/>.
    /// </summary>
    /// <remarks>
    ///     There is no known reason why these are different.
    ///     <see
    ///         cref="Lumina.Excel.GeneratedSheets2.TerritoryIntendedUse.Unknown11">
    ///         TerritoryIntendedUse.Unknown11
    ///     </see>
    ///     is the only difference, so discovering it's purpose may explain the reason.
    /// </remarks>
    Raid2 = 17,

    /// <summary>
    ///     Zones used for frontlines PvP.
    /// </summary>
    Frontlines = 18,

    /// <summary>
    ///     This used to be the territory with name "w1ed" (the zone under The Gold Saucer), but that has been changed
    ///     to use <see cref="TerritoryIntendedUseEnum.TheGoldSaucer"/>.
    /// </summary>
    /// <remarks>
    ///     No longer referenced in the sheets.
    /// </remarks>
    ChocoboSquareOld = 19,

    /// <summary>
    ///     The chocobo racing courses, and the tutorial zone.
    /// </summary>
    ChocoboRacing = 20,

    /// <summary>
    ///     The Firmament in Foundation.
    /// </summary>
    TheFirmament = 21,

    /// <summary>
    ///     This is the area where weddings happen.
    /// </summary>
    SanctumOfTheTwelve = 22,

    /// <summary>
    ///     The Gold Saucer zones.
    /// </summary>
    TheGoldSaucer = 23,

    /// <summary>
    ///     The original Steps of Faith trial used this. Steps of Faith was made a solo duty in patch 6.2 and now uses
    ///     <see cref="SoloDuty"/>.
    /// </summary>
    OriginalStepsOfFaith = 24,

    /// <summary>
    ///     Lord of Verminion matches.
    /// </summary>
    LordOfVerminion = 25,

    /// <summary>
    ///     The first version of The Diadem, released in patch 3.1.
    /// </summary>
    /// <remarks>
    ///     No longer accessible in game.
    /// </remarks>
    ExploratoryMissions = 26,

    /// <summary>
    ///     The Hall of the Novice training grounds.
    /// </summary>
    HallOfTheNovice = 27,

    /// <summary>
    ///     Crystalline Conflict and the Feast PvP zones.
    /// </summary>
    SmallScalePvp = 28,

    /// <summary>
    ///     Solo duties that are queued for.
    /// </summary>
    SoloDuty = 29,

    /// <summary>
    ///     Grand Company Barracks, where squadrons are accessed.
    /// </summary>
    GrandCompanyBarracks = 30,

    /// <summary>
    ///     Palace of the Dead, Heaven on High, and Eureka Orthos.
    /// </summary>
    DeepDungeon = 31,

    /// <summary>
    ///     The special zones that are only available during holidays.
    /// </summary>
    HolidayInstance = 32,

    /// <summary>
    ///     The inside of a Timeworn Treasure Map portal.
    /// </summary>
    MapPortal = 33,

    /// <summary>
    ///     Zones used for duties that are only available during holidays.
    /// </summary>
    HolidayDuty = 34,

    /// <summary>
    ///     The triple triad battlehall.
    /// </summary>
    TripleTriadBattlehall = 35,

    /// <summary>
    ///     Kugane Ohashi trial up until patch 4.36. The trial was moved to <see cref="Trial"/> in patch 4.4. The trial
    ///     didn't actually release until  patch 4.56, so this was never actually used.
    /// </summary>
    /// <remarks>
    ///     No longer accessible in game.
    /// </remarks>
    PreReleaseKuganeOhashi = 36,

    /// <summary>
    ///     Crystalline Conflict custom matches.
    /// </summary>
    PvpCustomMatch = 37,

    /// <summary>
    ///     Battle class only versions of the first Diadem.
    /// </summary>
    /// <remarks>
    ///     No longer accessible in game.
    /// </remarks>
    TheHuntingGrounds = 38,

    /// <summary>
    ///     Rival Wings PvP zones.
    /// </summary>
    RivalWings = 39,

    /// <summary>
    ///     A couple rooms that are only accessible during holidays use this. A copy of the Mordion Gaol from The Rising
    ///     and Frondale's Home for Friendless Foundlings
    /// </summary>
    HolidayEventRoom = 40,

    /// <summary>
    ///     All instanced Eureka zones.
    /// </summary>
    Eureka = 41,

    /// <summary>
    ///     Was used for the Feast map Crystal Tower Training Grounds. The map was removed in patch 6.1.
    /// </summary>
    /// <remarks>
    ///     No longer accessible in game.
    /// </remarks>
    CrystalTowerTrainingGrounds = 42,

    /// <summary>
    ///     Used for The Rising event in 2018.
    /// </summary>
    /// <remarks>
    ///     No longer accessible in game.
    /// </remarks>
    TheCalamityRetold = 43,

    /// <summary>
    ///     Gold Saucer GATE The Leap Of Faith maps.
    /// </summary>
    LeapOfFaith = 44,

    /// <summary>
    ///     The Blue Mage-only Masked Carnivale.
    /// </summary>
    MaskedCarnivale = 45,

    /// <summary>
    ///     Ocean fishing zones.
    /// </summary>
    OceanFishing = 46,

    /// <summary>
    ///     The reworked version of The Diadem released in patch 3.55.
    /// </summary>
    /// <remarks>
    ///     No longer accessible in game.
    /// </remarks>
    TheDiademRework = 47,

    /// <summary>
    ///     The Southern Front and Zadnor.
    /// </summary>
    Bozja = 48,

    /// <summary>
    ///     Island Sanctuary.
    /// </summary>
    /// <remarks>
    ///     This might be for all "lifestyle" content and will be renamed in the future if that is true.
    /// </remarks>
    IslandSanctuary = 49,

    /// <summary>
    ///     Triple Triad official tournaments.
    /// </summary>
    TripleTriadTournament = 50,

    /// <summary>
    ///     The Triple Triad Parlor. Non-official party tournaments.
    /// </summary>
    TripleTriadParlor = 51,

    /// <summary>
    ///     The normal mode difficulty of The Delubrum Reginae.
    /// </summary>
    DelebrumReginae = 52,

    /// <summary>
    ///     The savage mode difficulty of The Delubrum Reginae.
    /// </summary>
    DelebrumReginaeSavage = 53,

    /// <summary>
    ///     Seems to only be used in the The Propylaion in Elpis, and the in an Ultima Thule section. The Propylaion is
    ///     the initial zone you enter on your first visit to Elpis during the MSQ. It is inaccessible after that.
    /// </summary>
    EndwalkerMsqSoloOverworld = 54,

    /// <summary>
    ///     No <see cref="Lumina.Excel.GeneratedSheets2.TerritoryType"/> uses this currently.
    /// </summary>
    /// <remarks>
    ///     No history for this value being used.
    /// </remarks>
    CurrentlyUnused2 = 55,

    /// <summary>
    ///     Special zone for gathering materials for tribal quests.
    /// </summary>
    /// <remarks>
    ///     Currently only Elysion for the Omicron Tribe. I'm assuming it will be used again for future tribes.
    /// </remarks>
    TribalGathering = 56,

    /// <summary>
    ///     Normal mode difficulty of Criterion dungeons.
    /// </summary>
    CriterionDungeon = 57,

    /// <summary>
    ///     Savage mode difficulty of Criterion dungeons.
    /// </summary>
    CriterionDungeonSavage = 58,

    /// <summary>
    ///     The zone for the Blunderville event introduced in 6.51.
    /// </summary>
    /// <remarks>
    ///     There is no guarantee that this won't be used for future events, at which point this will be renamed.
    /// </remarks>
    Blunderville = 59,

    /// <summary>
    ///     No <see cref="Lumina.Excel.GeneratedSheets2.TerritoryType"/> uses this currently.
    /// </summary>
    CurrentlyUnused3 = 60,
}

#pragma warning disable SA1649
#pragma warning disable SA1402
public static class TerritoryIntendedUseExtensions
#pragma warning restore SA1402
#pragma warning restore SA1649
{
    public static bool HasAlliance(this TerritoryIntendedUse territory)
        => territory switch
        {
            TerritoryIntendedUse.AllianceRaid => true,
            TerritoryIntendedUse.Frontlines => true,
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
            TerritoryIntendedUse.DungeonsAndGuildhests => true,
            TerritoryIntendedUse.VariantDungeon => true,
            TerritoryIntendedUse.AllianceRaid => true,
            TerritoryIntendedUse.Trial => true,
            TerritoryIntendedUse.Raid1 => true,
            TerritoryIntendedUse.Raid2 => true,
            TerritoryIntendedUse.Frontlines => true,
            TerritoryIntendedUse.LordOfVerminion => true,
            TerritoryIntendedUse.SmallScalePvp => true,
            TerritoryIntendedUse.DeepDungeon => true,
            TerritoryIntendedUse.MapPortal => true,
            TerritoryIntendedUse.HolidayDuty => true,
            TerritoryIntendedUse.PvpCustomMatch => true,
            TerritoryIntendedUse.RivalWings => true,
            TerritoryIntendedUse.Eureka => true,
            TerritoryIntendedUse.MaskedCarnivale => true,
            TerritoryIntendedUse.TheDiademRework => true,
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
            TerritoryIntendedUse.Frontlines => AllianceType.SixParty,
            _ => AllianceType.None,
        };
}