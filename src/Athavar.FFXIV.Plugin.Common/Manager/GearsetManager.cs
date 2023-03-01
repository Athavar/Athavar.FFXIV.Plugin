// <copyright file="GearsetManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

internal class GearsetManager : IGearsetManager, IDisposable
{
    private const int StatLength = 75;

    private readonly IDalamudServices dalamudServices;
    private readonly ExcelSheet<Item> itemsSheet;
    private readonly ExcelSheet<Materia> materiaSheet;
    private readonly ExcelSheet<BaseParam> baseParamSheet;
    private readonly ExcelSheet<ItemLevel> itemLevelSheet;
    private readonly ExcelSheet<ClassJob> classJobSheet;

    public GearsetManager(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;
        var dataManager = dalamudServices.DataManager;
        var clientState = dalamudServices.ClientState;

        this.itemsSheet = dataManager.GetExcelSheet<Item>() ?? throw new AthavarPluginException();
        this.materiaSheet = dataManager.GetExcelSheet<Materia>() ?? throw new AthavarPluginException();
        this.baseParamSheet = dataManager.GetExcelSheet<BaseParam>() ?? throw new AthavarPluginException();
        this.itemLevelSheet = dataManager.GetExcelSheet<ItemLevel>() ?? throw new AthavarPluginException();
        this.classJobSheet = dataManager.GetExcelSheet<ClassJob>() ?? throw new AthavarPluginException();

        clientState.Login += this.ClientStateOnLogin;
        clientState.Logout += this.ClientStateOnLogout;

        if (clientState.IsLoggedIn)
        {
            this.UpdateGearsets();
        }
    }

    /// <inheritdoc />
    public IEnumerable<Gearset> AllGearsets => this.Gearsets;

    private List<Gearset> Gearsets { get; } = new();

    /// <inheritdoc />
    public void Dispose()
    {
        var clientState = this.dalamudServices.ClientState;
        clientState.Login -= this.ClientStateOnLogin;
        clientState.Logout -= this.ClientStateOnLogout;
    }

    public unsafe void EquipGearset(int gearsetId) => RaptureGearsetModule.Instance()->EquipGearset(gearsetId);

    /// <inheritdoc />
    public unsafe void UpdateGearsets()
    {
        this.Gearsets.Clear();

        if (!this.dalamudServices.ClientState.IsLoggedIn)
        {
            return;
        }

        var instance = RaptureGearsetModule.Instance();
        var levelArray = PlayerState.Instance()->ClassJobLevelArray;
        for (var i = 0; i < 100; ++i)
        {
            var gearsetEntryPtr = instance->Gearset[i];
            if ((nint)gearsetEntryPtr == nint.Zero || gearsetEntryPtr->ClassJob == 0 || (gearsetEntryPtr->Flags & RaptureGearsetModule.GearsetFlag.Exists) == 0)
            {
                continue;
            }

            var expArrayIndex = this.classJobSheet.GetRow(gearsetEntryPtr->ClassJob)?.ExpArrayIndex;
            if (expArrayIndex is not { } levelArrayIndex || levelArray[levelArrayIndex] == 0)
            {
                continue;
            }

            var stats = new uint[StatLength];
            this.GetItemStats(ref stats, &gearsetEntryPtr->MainHand);
            this.GetItemStats(ref stats, &gearsetEntryPtr->OffHand);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Head);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Body);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Hands);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Legs);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Feet);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Ears);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Neck);
            this.GetItemStats(ref stats, &gearsetEntryPtr->Wrists);
            this.GetItemStats(ref stats, &gearsetEntryPtr->RightLeft);
            this.GetItemStats(ref stats, &gearsetEntryPtr->RingRight);
            this.GetItemStats(ref stats, &gearsetEntryPtr->SoulStone);
            this.Gearsets.Add(new Gearset(Marshal.PtrToStringUTF8((nint)gearsetEntryPtr->Name) ?? "<???>", gearsetEntryPtr->ID, gearsetEntryPtr->ClassJob, (byte)levelArray[levelArrayIndex], stats, (&gearsetEntryPtr->SoulStone)->ItemID != 0));
        }
    }

    public Gearset? GetCurrentEquipment()
    {
        var stats = new uint[StatLength];
        var equipmentItems = this.CurrentEquipment();
        if (equipmentItems == null)
        {
            return null;
        }

        foreach (var inventoryItem in equipmentItems)
        {
            this.GetItemStats(ref stats, inventoryItem);
        }

        var player = this.dalamudServices.ClientState.LocalPlayer;

        return new Gearset("<???>", 0, player?.ClassJob.Id ?? 0, player?.Level ?? 0, stats, equipmentItems.Length >= 12 && equipmentItems[11].ItemID != 0);
    }

    private unsafe InventoryItem[]? CurrentEquipment()
    {
        var im = this.GetInventoryManager();
        var container = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (container is null)
        {
            return null;
        }

        var items = new InventoryItem[container->Size];
        for (var index = 0; index < container->Size; index++)
        {
            items[index] = *container->GetInventorySlot(index);
        }

        return items;
    }

    private void ClientStateOnLogin(object? sender, EventArgs e) => this.UpdateGearsets();

    private void ClientStateOnLogout(object? sender, EventArgs e) => this.Gearsets.Clear();

    private unsafe void GetItemStats(ref uint[] stats, RaptureGearsetModule.GearsetItem* item)
    {
        var materia = new (ushort Id, byte Grade)[5];
        for (var i = 0; i < 5; i++)
        {
            materia[i] = (item->Materia[i], item->MateriaGrade[i]);
        }

        this.GetItemStats(ref stats, item->ItemID, materia);
    }

    private unsafe void GetItemStats(ref uint[] stats, InventoryItem item)
    {
        var materia = new (ushort Id, byte Grade)[5];
        for (var i = 0; i < 5; i++)
        {
            materia[i] = (item.Materia[i], item.MateriaGrade[i]);
        }

        var itemId = item.ItemID;
        if ((item.Flags & InventoryItem.ItemFlags.HQ) != 0)
        {
            itemId += 1000000U;
        }

        this.GetItemStats(ref stats, itemId, materia);
    }

    private void GetItemStats(ref uint[] stats, uint itemId, (ushort Id, byte Grade)[]? materia = null)
    {
        var flag = itemId > 1000000U;
        var item = this.itemsSheet.GetRow(itemId % 1000000U);
        if (item?.LevelItem.Value is null)
        {
            return;
        }

        var tmpStats = new uint[StatLength];
        foreach (var itemUnkData59Obj in item.UnkData59)
        {
            tmpStats[itemUnkData59Obj.BaseParam] += Convert.ToUInt32(itemUnkData59Obj.BaseParamValue);
        }

        if (flag)
        {
            foreach (var itemUnkData73Obj in item.UnkData73)
            {
                tmpStats[itemUnkData73Obj.BaseParamSpecial] += Convert.ToUInt32(itemUnkData73Obj.BaseParamValueSpecial);
            }
        }

        var hasMateria = false;
        if (materia is not null)
        {
            foreach (var (id, grade) in materia)
            {
                if (id == 0)
                {
                    // no materia
                    continue;
                }

                var m = this.materiaSheet.GetRow(id);
                if (m != null)
                {
                    tmpStats[(byte)m.BaseParam.Row] += Convert.ToUInt32(m.Value[grade]);
                    hasMateria = true;
                }
            }
        }

        for (ushort statId = 0; statId < StatLength; statId++)
        {
            var value = tmpStats[statId];
            if (value == 0)
            {
                continue;
            }

            if (!hasMateria)
            {
                stats[statId] += value;
                continue;
            }

            var baseParam = this.baseParamSheet.GetRow(statId);
            if (baseParam is null)
            {
                throw new AthavarPluginException($"Fail to find baseParam for statId {statId}");
            }

            var ilvlBase = this.itemLevelSheet.GetRowParser(item.LevelItem.Row)?.ReadColumn<ushort>(statId - 1);
            if (ilvlBase is null)
            {
                throw new AthavarPluginException($"Fail to find ilvlBase for ilvl {item.LevelItem.Row} and statId {statId}");
            }

            var maxStat = this.GetMaxStat(baseParam, item, ilvlBase.Value);

            stats[statId] += Math.Min(maxStat, value);
        }
    }

    private uint GetMaxStat(BaseParam param, Item item, uint ilvlBase)
    {
        var equipSlotPercent = this.GetEquipSlotPercent(param, item.EquipSlotCategory.Row);
        var num = item.BaseParamModifier <= 12 ? param.MeldParam[item.BaseParamModifier] : 0;
        return (uint)Math.Round((ilvlBase * equipSlotPercent * num) / 100000.0, MidpointRounding.AwayFromZero);
    }

    private uint GetEquipSlotPercent(BaseParam param, uint category)
    {
        uint equipSlotPercent;
        switch (category)
        {
            case 1:
                equipSlotPercent = param.oneHWpnPct;
                break;
            case 2:
                equipSlotPercent = param.OHPct;
                break;
            case 3:
                equipSlotPercent = param.HeadPct;
                break;
            case 4:
                equipSlotPercent = param.ChestPct;
                break;
            case 5:
                equipSlotPercent = param.HandsPct;
                break;
            case 6:
                equipSlotPercent = param.WaistPct;
                break;
            case 7:
                equipSlotPercent = param.LegsPct;
                break;
            case 8:
                equipSlotPercent = param.FeetPct;
                break;
            case 9:
                equipSlotPercent = param.EarringPct;
                break;
            case 10:
                equipSlotPercent = param.NecklacePct;
                break;
            case 11:
                equipSlotPercent = param.BraceletPct;
                break;
            case 12:
                equipSlotPercent = param.RingPct;
                break;
            case 13:
                equipSlotPercent = param.twoHWpnPct;
                break;
            case 14:
                equipSlotPercent = param.oneHWpnPct;
                break;
            default:
                equipSlotPercent = 0U;
                break;
        }

        return equipSlotPercent;
    }

    private unsafe InventoryManager* GetInventoryManager() => InventoryManager.Instance();
}