// <copyright file="Opcode.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

// ReSharper disable once CheckNamespace
namespace Athavar.FFXIV.Plugin;

#pragma warning disable SA1602
public enum Opcode
{
    // opcodes from machina
    StatusEffectList = 1,
    StatusEffectListEureka = 0x01DE, // not in machina
    StatusEffectListBozja = 2, // machina: StatusEffectList2
    StatusEffectListPlayer = 3, // machina: StatusEffectList3
    StatusEffectListDouble = 4, // machina: BossStatusEffectList
    ActionEffect1 = 5, // FFXIVOpcodes calls this Effect, machina calls it AbilityN, size=124
    ActionEffect8 = 6, // FFXIVOpcodes calls this AoeEffectN, machina calls it AbilityN, size=636
    ActionEffect16 = 7,
    ActionEffect24 = 8,
    ActionEffect32 = 9,
    ActorCast = 10,
    EffectResult = 11,

    [Obsolete]
    EffectResult4 = 12,

    [Obsolete]
    EffectResult8 = 13,

    [Obsolete]
    EffectResult16 = 14,
    EffectResultBasic = 15,

    [Obsolete]
    EffectResultBasic4 = 16,

    [Obsolete]
    EffectResultBasic8 = 17,

    [Obsolete]
    EffectResultBasic16 = 18,

    [Obsolete]
    EffectResultBasic32 = 19,

    [Obsolete]
    EffectResultBasic64 = 20,
    ActorControl = 21, // look at toggle weapon
    ActorControlSelf = 22, // look at cooldown
    ActorControlTarget = 23, // look at target change
    UpdateHpMpTp = 24,
    PlayerSpawn = 25, // machina: PlayerSpawn
    NpcSpawn = 26, // machina: NpcSpawn
    SpawnBoss = 27, // machina: NpcSpawn2

    [Obsolete]
    DespawnCharacter = 28, // not in machina
    ActorMove = 29,
    ActorSetPos = 30,
    ActorGauge = 31,
    PlaceWaymarkPreset = 32, // FFXIVOpcodes calls this PlaceFieldMarkerPreset, machina: PresetWaymark
    PlaceWaymark = 33, // FFXIVOpcodes calls this PlaceFieldMarker
    SystemLogMessage = 34, // FFXIVOpcodes calls this SomeDirectorUnk4

    // opcodes from FFXIVOpcodes
    PlayerSetup = 35,
    UpdateClassInfo = 36,

    [Obsolete]
    UpdateClassInfoEureka = 37, // Not in FFXIVOpcodes, but should exist

    [Obsolete]
    UpdateClassInfoBozja = 38, // Not in FFXIVOpcodes, but should exist
    PlayerStats = 39,
    Playtime = 40,
    UpdateSearchInfo = 41,
    ExamineSearchInfo = 42,
    Examine = 43,
    CurrencyCrystalInfo = 44,
    InitZone = 45,
    WeatherChange = 46,
    HousingWardInfo = 47,
    PrepareZoning = 48,
    ContainerInfo = 49,
    ItemInfo = 50,
    DesynthResult = 51,
    FreeCompanyInfo = 52,
    FreeCompanyDialog = 53,
    MarketBoardSearchResult = 54,
    MarketBoardItemListingCount = 55,
    MarketBoardItemListingHistory = 56,
    MarketBoardItemListing = 57,
    MarketBoardPurchase = 58,
    UpdateInventorySlot = 59,
    InventoryActionAck = 60,
    InventoryTransaction = 61,
    InventoryTransactionFinish = 62,
    ResultDialog = 63,
    RetainerInformation = 64,
    ItemMarketBoardInfo = 65,
    EventStart = 66,
    EventFinish = 67,
    CFPreferredRole = 68,
    CFNotify = 69,
    ObjectSpawn = 70,
    AirshipTimers = 71,
    SubmarineTimers = 72,
    AirshipStatusList = 73,
    AirshipStatus = 74,
    AirshipExplorationResult = 75,
    SubmarineProgressionStatus = 76,
    SubmarineStatusList = 77,
    SubmarineExplorationResult = 78,
    EventPlay = 79,
    EventPlay4 = 80,

    [Obsolete]
    EventPlay8 = 81,

    [Obsolete]
    EventPlay16 = 82,
    EventPlay32 = 83,
    EventPlay64 = 84,

    [Obsolete]
    EventPlay128 = 85,

    [Obsolete]
    EventPlay255 = 86,

    [Obsolete]
    Logout = 87,
    IslandWorkshopSupplyDemand = 88,
    MiniCactpotInit = 89,
    SocialList = 90,
    FateInfo = 91,

    // Client -> Server
    UpdatePositionHandler = 92,
    ClientTrigger = 93,
    ChatHandler = 94,
    SetSearchInfoHandler = 95,
    MarketBoardPurchaseHandler = 96,
    InventoryModifyHandler = 97,
    UpdatePositionInstance = 98,
}
#pragma warning restore SA1602