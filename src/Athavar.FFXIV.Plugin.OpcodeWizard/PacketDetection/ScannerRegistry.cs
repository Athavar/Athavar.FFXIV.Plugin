// <copyright file="ScannerRegistry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.OpcodeWizard.Models;
using FFXIVClientStructs.FFXIV.Client.Game;

internal sealed class ScannerRegistry
{
    private readonly IList<Scanner> scanners;
    private readonly IOpcodeManager opcodeManager;
    private readonly IDalamudServices dalamudServices;

    public ScannerRegistry(IOpcodeManager opcodeManager, IDalamudServices dalamudServices)
    {
        this.opcodeManager = opcodeManager;
        this.dalamudServices = dalamudServices;
        this.scanners = new List<Scanner>();
        this.DeclareScanners();
    }

    public IList<Scanner> AsList() => this.scanners.ToList();

    public Scanner? GetScanner(int index)
    {
        if (index >= this.scanners.Count || index < 0)
        {
            return null;
        }

        return this.scanners[index];
    }

    private static bool IncludesBytes(byte[] source, byte[] search)
    {
        if (search == null)
        {
            return false;
        }

        for (var i = 0; i < source.Length - search.Length; ++i)
        {
            var result = true;
            for (var j = 0; j < search.Length; ++j)
            {
                if (search[j] != source[i + j])
                {
                    result = false;
                    break;
                }
            }

            if (result)
            {
                return true;
            }
        }

        return false;
    }

    [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1005:Single line comments should begin with single space", Justification = "look")]
    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1117:Parameters should be on same line or separate lines", Justification = "look")]
    [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1515:Single-line comment should be preceded by blank line", Justification = "look")]
    private void DeclareScanners()
    {
        var inArray = (uint[] arr, uint item) => arr.Any(i => i == item);

        //=================
        this.RegisterScanner(Opcode.PlayerSetup, "Please log in.",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize > 300 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])),
            new[] { "Please enter your character name:" });

        //=================
        var maxHp = 0;
        this.RegisterScanner(Opcode.UpdateHpMpTp, "Please alter your HP or MP and allow your stats to regenerate completely.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                maxHp = int.Parse(parameters[0]);

                if (packet.PacketSize != 40 && packet.PacketSize != 48)
                {
                    return false;
                }

                var packetHp = BitConverter.ToUInt32(packet.Data, Offsets.IpcData);
                var packetMp = BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 4);

                return packetHp == maxHp && packetMp == 10000;
            }, new[] { "Please enter your max HP:" });
        //=================
        this.RegisterScanner(Opcode.UpdateClassInfo, "Switch to the job you entered level for.",
            PacketSource.Server, (packet, parameters) =>
                packet.PacketSize == 48 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 4) ==
                int.Parse(parameters[0]), new[] { "Please enter your the level for another job:" }, Opcode.UpdateHpMpTp);
        //=================
        this.RegisterScanner(Opcode.PlayerStats, "Switch back to the job you entered HP for.",
            PacketSource.Server, (packet, parameters) =>
                packet.PacketSize == 256 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 24) == maxHp &&
                BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 28) == 10000 && // MP equals 10000
                BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 36) == 10000, dependency: Opcode.UpdateHpMpTp); // GP equals 10000
        //=================
        this.RegisterScanner(Opcode.UpdatePositionHandler, "Please move your character.",
            PacketSource.Client,
            (packet, _) => packet.PacketSize == 56 &&
                           packet.SourceActor == packet.TargetActor &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 4) == 0 &&
                           BitConverter.ToUInt64(packet.Data, Offsets.IpcData + 8) != 0 &&
                           BitConverter.ToUInt32(packet.Data, packet.Data.Length - 4) == 0);
        //=================
        this.RegisterScanner(Opcode.ClientTrigger, "Please draw your weapon.",
            PacketSource.Client,
            (packet, _) =>
                packet.PacketSize == 64 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData) == 1);
        this.RegisterScanner(Opcode.ActorControl, "Please draw your weapon.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 56 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 4) == 1);
        //=================
        this.RegisterScanner(Opcode.ActorControlSelf, "Please enter sanctuary and wait for rested bonus gains.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 64 &&
                           BitConverter.ToUInt16(packet.Data, Offsets.IpcData) == 24 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 4) <= 604800 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == 0 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 12) == 0 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16) == 0 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 20) == 0 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 24) == 0);
        //=================
        this.RegisterScanner(Opcode.ActorControlTarget, "Please mark yourself with the \"1\" marker.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 64 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x04) == 0 &&
                           packet.SourceActor == packet.TargetActor &&
                           packet.SourceActor == BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x08) &&
                           packet.SourceActor == BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x18));
        //=================
        this.RegisterScanner(Opcode.ChatHandler, "Please /say your message in-game:",
            PacketSource.Client,
            (packet, parameters) => IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])),
            new[] { "Please enter a message to /say in-game:" });
        //=================
        this.RegisterScanner(Opcode.Playtime, "Please type /playtime.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                if (packet.PacketSize != 40 || packet.SourceActor != packet.TargetActor)
                {
                    return false;
                }

                var playtime = BitConverter.ToUInt32(packet.Data, Offsets.IpcData);

                var inputDays = int.Parse(parameters[0]);
                var packetDays = playtime / 60 / 24;

                // In case you played for 23:59:59
                return inputDays == packetDays || inputDays + 1 == packetDays;
            }, new[] { "Type /playtime, and input the days you played:" });
        //=================
        byte[]? searchBytes = null;
        this.RegisterScanner(Opcode.SetSearchInfoHandler, "Please set that search comment in-game.",
            PacketSource.Client,
            (packet, parameters) =>
            {
                searchBytes ??= Encoding.UTF8.GetBytes(parameters[0]);
                return IncludesBytes(packet.Data, searchBytes);
            }, new[] { "Please enter a somewhat lengthy search message here, before entering it in-game:" });
        this.RegisterScanner(Opcode.UpdateSearchInfo, string.Empty,
            PacketSource.Server,
            (packet, _) => IncludesBytes(packet.Data, searchBytes!), dependency: Opcode.SetSearchInfoHandler);
        this.RegisterScanner(Opcode.ExamineSearchInfo, "Open your search information with the \"View Search Info\" button.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize > 232 && IncludesBytes(packet.Data, searchBytes!), dependency: Opcode.UpdateSearchInfo);
        //=================
        this.RegisterScanner(Opcode.Examine, "Please examine that character's equipment.",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 1016 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])),
            new[] { "Please enter a nearby character's name:" });
        //=================
        var lightningCrystals = -1;
        this.RegisterScanner(Opcode.ActorCast, "Please teleport to Limsa Lominsa Lower Decks.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                if (lightningCrystals == -1 && parameters.Length > 0)
                {
                    lightningCrystals = int.Parse(parameters[0]);
                }

                return packet.PacketSize == 64 &&
                       BitConverter.ToUInt16(packet.Data, Offsets.IpcData) == 5;
            });

        this.RegisterScanner(Opcode.CurrencyCrystalInfo, "Please wait. This is updated during territory change.",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 56)
                {
                    return false;
                }

                unsafe
                {
                    return BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 4) == 2001 &&
                           BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 6) == 10 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16) == 12 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == InventoryManager.Instance()->GetInventoryItemCount(12);
                }
            });
        this.RegisterScanner(Opcode.InitZone, string.Empty, PacketSource.Server,
            (packet, _) => packet.PacketSize == 136 &&
                           BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 2) == 129);
        uint[] limsaLominsaWeathers = { 3, 1, 2, 4, 7 };
        this.RegisterScanner(Opcode.WeatherChange, string.Empty, PacketSource.Server,
            (packet, _) => packet.PacketSize == 40 &&
                           inArray(limsaLominsaWeathers, packet.Data[Offsets.IpcData]) &&
                           BitConverter.ToSingle(packet.Data, Offsets.IpcData + 4) == 20.0);
        //=================
        var actorMoveCenter = new Vector3(-85f, 19f, 0);
        var inRange = (Vector3 diff, Vector3 range) => { return Math.Abs(diff.X) < range.X && Math.Abs(diff.Y) < range.Y && Math.Abs(diff.Z) < range.Z; };
        this.RegisterScanner(Opcode.ActorMove, "Please wait. (Teleport to Limsa Lominsa Lower Decks if you haven't)",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 48)
                {
                    return false;
                }

                var x = (((float)BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 6) / 65536) * 2000) - 1000;
                var y = (((float)BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 8) / 65536) * 2000) - 1000;
                var z = (((float)BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 12) / 65536) * 2000) - 1000;

                return inRange(new Vector3(x, y, z) - actorMoveCenter, new Vector3(15, 2, 15));
            });
        //=================
        this.RegisterScanner(Opcode.PlayerSpawn, "Please wait for another player to spawn in your vicinity.",
            PacketSource.Server, (packet, parameters) =>
            {
                if (packet.PacketSize <= 500)
                {
                    return false;
                }

                var world = this.dalamudServices.ClientState.LocalPlayer?.CurrentWorld.Id;
                if (world is null)
                {
                    return false;
                }

                return BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 4) == world;
            });
        /* Commented for now because this also matches UpdateTpHpMp
        RegisterScanner("ActorFreeSpawn", string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 40 &&
                           packet.SourceActor == packet.TargetActor);
        */
        //=================
        this.RegisterScanner(Opcode.ActorSetPos, "Please wait, this may take some time. You can also teleport to another Aethernet Shard in the same map and then teleport back.",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 56)
                {
                    return false;
                }

                var x = BitConverter.ToSingle(packet.Data, Offsets.IpcData + 8);
                var y = BitConverter.ToSingle(packet.Data, Offsets.IpcData + 12);
                var z = BitConverter.ToSingle(packet.Data, Offsets.IpcData + 16);

                return inRange(new Vector3(x, y, z) - actorMoveCenter, new Vector3(15, 2, 15));
            });
        //=================
        this.RegisterScanner(Opcode.HousingWardInfo, "Please view a housing ward from a city aetheryte/ferry.",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 2448 &&
                                    IncludesBytes(new ArraySegment<byte>(packet.Data, Offsets.IpcData + 16, 32).ToArray(), Encoding.UTF8.GetBytes(parameters[0])),
            new[] { "Please enter the name of whoever owns the first house in the ward (if it's an FC, their shortname):" });
        //=================
        this.RegisterScanner(Opcode.PrepareZoning, "Please teleport to The Aftcastle (Adventurers' Guild in Limsa Lominsa Upper Decks).",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 48)
                {
                    return false;
                }

                var logMessage = BitConverter.ToUInt32(packet.Data, Offsets.IpcData);
                var targetZone = BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 4);
                var animation = BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 6);
                var fadeOutTime = packet.Data[Offsets.IpcData + 10];

                return logMessage == 0 &&
                       targetZone == 128 &&
                       animation == 112 &&
                       fadeOutTime == 15 &&
                       packet.SourceActor == packet.TargetActor;
            });
        //=================
        this.RegisterScanner(Opcode.ContainerInfo, "Please wait.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == 2001);
        this.RegisterScanner(Opcode.ItemInfo, "Please open your chocobo saddlebag.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 96 &&
                           BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 8) == 4000);
        //=================
        this.RegisterScanner(Opcode.PlaceWaymark, "Please target The Aftcastle Aethernet Shard and type /waymark A <t>",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 &&
                           packet.SourceActor == packet.TargetActor &&
                           BitConverter.ToUInt16(packet.Data, Offsets.IpcData) == 0x0100 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x04) == 0x3edc &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x08) == 0x9c70);
        //=================
        this.RegisterScanner(Opcode.PlaceWaymarkPreset, "Please type /waymark clear",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 136 || packet.SourceActor != packet.TargetActor)
                {
                    return false;
                }

                for (var i = 0; i < 24; i++)
                {
                    if (BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x04 + (4 * i)) != 0)
                    {
                        return false;
                    }
                }

                return true;
            });
        //=================
        this.RegisterScanner(Opcode.EffectResult, "Switch to Fisher and enable snagging.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 128 &&
                           packet.SourceActor == packet.TargetActor &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == packet.SourceActor &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 12) ==
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16) &&
                           BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 0x1E) == 761);
        //=================
        this.RegisterScanner(Opcode.EventStart, "Please begin fishing and put your rod away immediately.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 56 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == 0x150001);
        this.RegisterScanner(Opcode.EventPlay, string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 72 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == 0x150001);
        this.RegisterScanner(Opcode.EventFinish, string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData) == 0x150001 &&
                           packet.Data[Offsets.IpcData + 4] == 0x14 &&
                           packet.Data[Offsets.IpcData + 5] == 0x01);
        //=================
        /*this.RegisterScanner(Opcode.SystemLogMessage, "Please cast your line and catch a fish at Limsa Lominsa.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 56 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x08) == 257);*/
        this.RegisterScanner(Opcode.EventPlay4, string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 80 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x1C) == 284, dependency: Opcode.SystemLogMessage);
        //=================
        uint[] limsaLominsaFishes = { 4869, 4870, 4776, 4871, 4872, 4874, 4876 };
        uint[] desynthResult = { 5267, 5823 };
        this.RegisterScanner(Opcode.DesynthResult, "Please desynth the fish (You can also purchase a Merlthor Goby, Lominsan Anchovy or Harbor Herring from marketboard). If you got items other than Fine Sand and Allagan Tin Piece, please desynth again.",
            PacketSource.Server,
            (packet, _) => (packet.PacketSize == 104 || packet.PacketSize == 136) &&
                           inArray(limsaLominsaFishes, BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x08) % 1000000) &&
                           inArray(desynthResult, BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x0C) % 1000000));
        //=================
        var fcRank = 0;
        this.RegisterScanner(Opcode.FreeCompanyInfo, "Load a zone. (If you are running scanners by order, suggest teleporting to Aetheryte Plaza)",
            PacketSource.Server,
            (packet, parameters) =>
            {
                fcRank = int.Parse(parameters[0]);
                return packet.PacketSize == 112 && packet.Data[Offsets.IpcData + 45] == fcRank;
            },
            new[] { "Please enter your Free Company rank:" });
        this.RegisterScanner(Opcode.FreeCompanyDialog, "Open your Free Company window (press G or ;)",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 112 && packet.Data[Offsets.IpcData + 0x31] == fcRank);
        //=================
        uint[] darkMatter = { 5594, 5595, 5596, 5597, 5598, 10386, 17837, 33916 };
        var isDarkMatter = (uint itemId) => inArray(darkMatter, itemId);

        this.RegisterScanner(Opcode.MarketBoardSearchResult, "Please click \"Catalysts\" on the market board.",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 208)
                {
                    return false;
                }

                for (var i = 0; i < 22; ++i)
                {
                    var itemId = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + (8 * i));
                    if (itemId == 0)
                    {
                        break;
                    }

                    if (itemId == darkMatter[6])
                    {
                        return true;
                    }
                }

                return false;
            });
        this.RegisterScanner(Opcode.MarketBoardItemListingCount, "Please open the market board listings for any Dark Matter.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 &&
                           isDarkMatter(BitConverter.ToUInt32(packet.Data, Offsets.IpcData)));
        this.RegisterScanner(Opcode.MarketBoardItemListingHistory, string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 1080 &&
                           isDarkMatter(BitConverter.ToUInt32(packet.Data, Offsets.IpcData)), dependency: Opcode.MarketBoardItemListingCount);
        this.RegisterScanner(Opcode.MarketBoardItemListing, string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize > 1552 &&
                           isDarkMatter(BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 44)), dependency: Opcode.MarketBoardItemListingCount);
        this.RegisterScanner(Opcode.MarketBoardPurchaseHandler, "Please purchase any Dark Matter",
            PacketSource.Client,
            (packet, _) => packet.PacketSize == 72 &&
                           isDarkMatter(BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x10)));
        this.RegisterScanner(Opcode.MarketBoardPurchase, string.Empty,
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 &&
                           isDarkMatter(BitConverter.ToUInt32(packet.Data, Offsets.IpcData)), dependency: Opcode.MarketBoardPurchaseHandler);
        //=================
        const uint scannerItemId = 4850; // Honey
        this.RegisterScanner(Opcode.UpdateInventorySlot, "Please purchase a Honey from Tradecraft Supplier (2 gil).",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 96 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x10) == scannerItemId);
        //=================
        uint inventoryModifyHandlerId = 0;
        this.RegisterScanner(Opcode.InventoryModifyHandler, "Please drop the Honey.",
            PacketSource.Client,
            (packet, _, comment) =>
            {
                var match = packet.PacketSize == 80 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 0x18) == scannerItemId;
                if (!match)
                {
                    return false;
                }

                inventoryModifyHandlerId = BitConverter.ToUInt32(packet.Data, Offsets.IpcData);

                var baseOffset = BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 4);
                comment.Text = $"Base offset: {Util.NumberToString(baseOffset, NumberDisplayFormat.HexadecimalUppercase)}";
                return true;
            });
        this.RegisterScanner(Opcode.InventoryActionAck, "Please wait.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData) == inventoryModifyHandlerId, dependency: Opcode.InventoryModifyHandler);
        this.RegisterScanner(Opcode.InventoryTransaction, "Please wait.",
            PacketSource.Server,
            (packet, _) =>
            {
                var match = packet.PacketSize == 80 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 0x18) == scannerItemId;
                if (!match)
                {
                    return false;
                }

                inventoryModifyHandlerId = BitConverter.ToUInt32(packet.Data, Offsets.IpcData);
                return true;
            }, dependency: Opcode.InventoryModifyHandler);
        this.RegisterScanner(Opcode.InventoryTransactionFinish, "Please wait.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData) == inventoryModifyHandlerId, dependency: Opcode.InventoryModifyHandler);
        //=================
        this.RegisterScanner(Opcode.ResultDialog, "Please visit a retainer counter and request information about market tax rates.",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.PacketSize != 72)
                {
                    return false;
                }

                var rate1 = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8);
                var rate2 = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 12);
                var rate3 = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16);
                var rate4 = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 20);

                return rate1 <= 7 && rate2 <= 7 && rate3 <= 7 && rate4 <= 7;
            });
        //=================
        byte[]? retainerBytes = null;
        this.RegisterScanner(Opcode.RetainerInformation, "Please use the Summoning Bell.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                retainerBytes ??= Encoding.UTF8.GetBytes(parameters[0]);
                return packet.PacketSize == 112 && IncludesBytes(packet.Data.Skip(73).Take(32).ToArray(), retainerBytes);
            }, new[] { "Please enter one of your retainers' names:" });
        //=================
        this.RegisterScanner(Opcode.NpcSpawn, "Please summon that retainer.",
            PacketSource.Server,
            (packet, parameters) => retainerBytes is not null && packet.PacketSize > 624 &&
                                    IncludesBytes(packet.Data.Skip(588).Take(36).ToArray(), retainerBytes), dependency: Opcode.RetainerInformation);
        //================
        this.RegisterScanner(Opcode.ItemMarketBoardInfo, "Please put any item on sale for a unit price of 123456 and summon the retainer again",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 64 &&
                                    BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 0x10) == 123456);
        //=================
        this.RegisterScanner(Opcode.ObjectSpawn, "Please enter a furnished house.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 96 &&
                           packet.Data[Offsets.IpcData + 1] == 12 &&
                           packet.Data[Offsets.IpcData + 2] == 4 &&
                           packet.Data[Offsets.IpcData + 3] == 0 &&
                           BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 12) == 0);
        //=================
        uint[] basicSynthesis = { 100001, 100015, 100030, 100045, 100060, 100075, 100090, 100105 };
        this.RegisterScanner(Opcode.EventPlay32, "Use Trial Synthesis from any recipes, and use Basic Synthesis",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 192 && inArray(basicSynthesis, BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 44)));
        //=================
        this.RegisterScanner(Opcode.EffectResultBasic, "Switch to White Mage, and auto attack on an enemy.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 56 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8) == packet.SourceActor);
        //=================
        this.RegisterScanner(Opcode.ActionEffect1, "Cast Dia on an enemy. Then wait for a damage tick.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 156 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 8) == 16532);
        //=================
        this.RegisterScanner(Opcode.StatusEffectList, "Please wait...",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 416 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 20) == 1871, dependency: Opcode.ActionEffect1);
        //=================
        this.RegisterScanner(Opcode.ActorGauge, "Wait for gauge changes, then clear the lilies.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 48 &&
                           packet.Data[Offsets.IpcData] == 24 &&
                           packet.Data[Offsets.IpcData + 5] == 0 &&
                           packet.Data[Offsets.IpcData + 6] > 0,
            dependency: Opcode.ActionEffect1);
        //=================
        this.RegisterScanner(Opcode.CFPreferredRole, "Please wait, this may take some time...", PacketSource.Server, (packet, _) =>
        {
            if (packet.PacketSize != 48)
            {
                return false;
            }

            var allInRange = true;

            for (var i = 1; i < 10; i++)
            {
                if (packet.Data[Offsets.IpcData + i] > 4 || packet.Data[Offsets.IpcData + i] < 1)
                {
                    allInRange = false;
                }
            }

            return allInRange;
        });
        //=================
        this.RegisterScanner(Opcode.CFNotify, "Please enter the \"Sastasha\" as an undersized party.", // CFNotifyPop
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 72 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 28) == 4);
        //=================
        this.RegisterScanner(Opcode.UpdatePositionInstance, "Please move your character in an/the instance.",
            PacketSource.Client,
            (packet, _) => packet.PacketSize == 72 &&
                           packet.SourceActor == packet.TargetActor &&
                           BitConverter.ToUInt64(packet.Data, Offsets.IpcData) != 0 &&
                           BitConverter.ToUInt64(packet.Data, Offsets.IpcData + 0x08) != 0 &&
                           BitConverter.ToUInt64(packet.Data, Offsets.IpcData + 0x10) != 0 &&
                           BitConverter.ToUInt64(packet.Data, Offsets.IpcData + 0x18) != 0 &&
                           BitConverter.ToUInt32(packet.Data, packet.Data.Length - 4) == 0);
        //=================
        uint[] whmHoly = { 139, 25860 };
        var isHolyPacket = (IpcPacket packet, uint packetSize) => packet.PacketSize == packetSize && inArray(whmHoly, BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 8));

        this.RegisterScanner(Opcode.ActionEffect8, "Attack multiple enemies with Holy.",
            PacketSource.Server,
            (packet, _) => isHolyPacket(packet, 668));
        //=================
        this.RegisterScanner(Opcode.ActionEffect16, "Attack multiple enemies (>8) with Holy.",
            PacketSource.Server,
            (packet, _) => isHolyPacket(packet, 1244));
        //=================
        this.RegisterScanner(Opcode.ActionEffect24, "Attack multiple enemies (>16) with Holy.",
            PacketSource.Server,
            (packet, _) => isHolyPacket(packet, 1820));
        //=================
        this.RegisterScanner(Opcode.ActionEffect32, "Attack multiple enemies (>24) with Holy.",
            PacketSource.Server,
            (packet, _) => isHolyPacket(packet, 2396));
        //=================
        this.RegisterScanner(Opcode.SystemLogMessage, "Please go to first boss room and touch any coral formation.",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 56 &&
                                    inArray(new uint[] { 2034, 2035 }, BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 4)));
        //=================
        string? airshipName = null;
        string? submarineName = null;

        this.RegisterScanner(Opcode.AirshipTimers, "Open your Estate tab from the Timers window if you have any airships on exploration.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                airshipName = parameters[0];
                return packet.PacketSize == 176 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(airshipName));
            },
            new[] { "Please enter your airship name:" });
        this.RegisterScanner(Opcode.SubmarineTimers, "Open your Estate tab from the Timers window if you have any submarines on exploration.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                submarineName = parameters[0];
                return packet.PacketSize == 176 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(submarineName));
            },
            new[] { "Please enter your submarine name:" });
        this.RegisterScanner(Opcode.AirshipStatusList, "Open your airship management console if you have any airships",
            PacketSource.Server,
            (packet, parameters) => airshipName is not null && packet.PacketSize == 192 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(airshipName)),
            dependency: Opcode.AirshipTimers);
        this.RegisterScanner(Opcode.AirshipStatus, "Check the status of a specific airship if you have any airships",
            PacketSource.Server,
            (packet, parameters) => airshipName is not null && packet.PacketSize == 104 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(airshipName)),
            dependency: Opcode.AirshipTimers);
        this.RegisterScanner(Opcode.AirshipExplorationResult, "Open a voyage log from an airship",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 320 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 4) == int.Parse(parameters[0]),
            new[] { "Please enter the experience from the first sector (first destination in log, not the ones next to report rank and items):" });
        this.RegisterScanner(Opcode.SubmarineProgressionStatus, "Open your submarine management console if you have any submarines",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 64 && packet.Data[Offsets.IpcData] >= 1 && packet.Data[Offsets.IpcData] <= 4);
        this.RegisterScanner(Opcode.SubmarineStatusList, "Open your submarine management console if you have any submarines",
            PacketSource.Server,
            (packet, parameters) => submarineName is not null && packet.PacketSize == 272 && IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(submarineName)),
            dependency: Opcode.SubmarineTimers);
        this.RegisterScanner(Opcode.SubmarineExplorationResult, "Open a voyage log from a submarine",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 320 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16) == int.Parse(parameters[0]),
            new[] { "Please enter the experience from the first sector (first destination in log, not the ones next to report rank and items):" });
        //=================
        this.RegisterScanner(Opcode.IslandWorkshopSupplyDemand, "Go to your Island Sanctuary and check workshop supply/demand status",
            PacketSource.Server,
            (packet, parameters) => packet.PacketSize == 116 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData) == 0 && BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 1) == 0);
        this.RegisterScanner(Opcode.MiniCactpotInit, "Start playing Mini Cactpot.",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.Data.Length != Offsets.IpcData + 136)
                {
                    return false;
                }

                var indexEnd = packet.Data[Offsets.IpcData + 7];
                var column = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 12);
                var row = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16);
                var digit = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 20);

                return indexEnd == 23 &&
                       column <= 2 &&
                       row <= 2 &&
                       digit <= 9;
            });
        //=================
        this.RegisterScanner(Opcode.SocialList, "Open your Party List.",
            PacketSource.Server,
            (packet, parameters) =>
            {
                if (packet.Data.Length != Offsets.IpcData + 896)
                {
                    return false;
                }

                if (packet.Data[(Offsets.IpcData + 13) - 1] != 1)
                {
                    return false;
                }

                if (!IncludesBytes(packet.Data, Encoding.UTF8.GetBytes(parameters[0])))
                {
                    return false;
                }

                return true;
            },
            new[] { "Please enter your character name:" });
        //=================
        var fateDurations = new uint[] { 15 * 60, 30 * 60 };
        this.RegisterScanner(Opcode.FateInfo, "Please enter a zone with F.A.T.E. If you are already in one, please wait for a new F.A.T.E. shows up.",
            PacketSource.Server,
            (packet, _) =>
            {
                if (packet.Data.Length != Offsets.IpcData + 24)
                {
                    return false;
                }

                var startTime = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 8);
                var duration = BitConverter.ToUInt32(packet.Data, Offsets.IpcData + 16);

                if (!inArray(fateDurations, duration))
                {
                    return false;
                }

                var endTime = startTime + duration;

                // Allow local time.
                var now = DateTimeOffset.Now.ToUnixTimeSeconds();
                var allowableError = 60;

                return now >= startTime - allowableError && now < endTime + allowableError;
            });
        /*RegisterScanner("StatusEffectList3", "Go to bozjan southern front instance. Cast Dia(WMA) on an enemy. Then wait for a damage tick.",
            PacketSource.Server,
            (packet, _) => packet.PacketSize == 800 && BitConverter.ToUInt16(packet.Data, Offsets.IpcData + 20) == 1871);*/
    }

    /// <summary>
    ///     Adds a scanner to the scanner registry.
    /// </summary>
    /// <param name="packetName">The name (Sapphire-style) of the packet.</param>
    /// <param name="tutorial">How the packet's conditions are created.</param>
    /// <param name="source">Whether the packet originates on the client or the server.</param>
    /// <param name="del">A boolean function that returns true if a packet matches the contained heuristics.</param>
    /// <param name="paramPrompts">An array of requests for auxiliary data that will be passed into the detection delegate.</param>
    /// <param name="dependency">Dependent Opcode scanner.</param>
    private void RegisterScanner(
        Opcode packetName,
        string tutorial,
        PacketSource source,
        Func<IpcPacket, string[], Comment, bool> del,
        string[]? paramPrompts = null,
        Opcode? dependency = null)
        => this.scanners.Add(
            new Scanner(
                packetName,
                packetName.AsText(),
                tutorial,
                del,
                new Comment(),
                paramPrompts ?? new string[] { },
                source,
                dependency)
            {
                Opcode = this.opcodeManager.GetOpcode(packetName),
            });

    private void RegisterScanner(
        Opcode packetName,
        string tutorial,
        PacketSource source,
        Func<IpcPacket, string[], bool> del,
        string[]? paramPrompts = null,
        Opcode? dependency = null)
    {
        bool Fn(IpcPacket a, string[] b, Comment c) => del(a, b);
        this.RegisterScanner(packetName, tutorial, source, Fn, paramPrompts, dependency);
    }
}