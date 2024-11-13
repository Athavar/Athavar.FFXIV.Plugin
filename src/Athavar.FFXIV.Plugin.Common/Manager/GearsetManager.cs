// <copyright file="GearsetManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Constants;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using Lumina.Excel;
using Lumina.Excel.Sheets;

internal sealed class GearsetManager : IGearsetManager, IDisposable
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
            this.dalamudServices.Framework.RunOnFrameworkThread(this.UpdateGearsets);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<Gearset> AllGearsets => this.Gearsets;

    private List<Gearset> Gearsets { get; } = new();

    /// <inheritdoc/>
    public void Dispose()
    {
        var clientState = this.dalamudServices.ClientState;
        clientState.Login -= this.ClientStateOnLogin;
        clientState.Logout -= this.ClientStateOnLogout;
    }

    public unsafe void EquipGearset(int gearsetId)
    {
        var gearsetModule = this.GetGearsetModule();
        if (gearsetId >= 0 && gearsetModule->IsValidGearset(gearsetId))
        {
            gearsetModule->EquipGearset(gearsetId);
        }
    }

    /// <inheritdoc/>
    public unsafe void UpdateGearsets()
    {
        this.Gearsets.Clear();

        if (!this.dalamudServices.ClientState.IsLoggedIn)
        {
            return;
        }

        var instance = this.GetGearsetModule();
        var levelArray = PlayerState.Instance()->ClassJobLevels;
        for (var i = 0; i < 100; ++i)
        {
            var gearsetEntryPtr = instance->GetGearset(i);
            if ((nint)gearsetEntryPtr == nint.Zero || gearsetEntryPtr->ClassJob == 0 || (gearsetEntryPtr->Flags & RaptureGearsetModule.GearsetFlag.Exists) == 0)
            {
                continue;
            }

            var expArrayIndex = this.classJobSheet.GetRow(gearsetEntryPtr->ClassJob).ExpArrayIndex;
            if (expArrayIndex < 0 || levelArray[expArrayIndex] == 0)
            {
                continue;
            }

            var stats = new uint[StatLength];
            List<uint> itemIds = new();
            foreach (var item in gearsetEntryPtr->Items)
            {
                itemIds.Add(item.ItemId);
                this.GetItemStats(ref stats, &item);
            }

            /*
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
            this.GetItemStats(ref stats, &gearsetEntryPtr->RingLeft);
            this.GetItemStats(ref stats, &gearsetEntryPtr->RingRight);
            this.GetItemStats(ref stats, &gearsetEntryPtr->SoulStone);*/
            this.Gearsets.Add(
                new Gearset(
                    this.GetName(gearsetEntryPtr),
                    gearsetEntryPtr->Id,
                    gearsetEntryPtr->ClassJob,
                    (byte)levelArray[expArrayIndex],
                    stats,
                    gearsetEntryPtr->Items[(int)RaptureGearsetModule.GearsetItemIndex.SoulStone].ItemId != 0,
                    gearsetEntryPtr->Items[(int)RaptureGearsetModule.GearsetItemIndex.MainHand].ItemId,
                    itemIds));
        }
    }

    public unsafe Gearset? GetCurrentEquipment()
    {
        var stats = new uint[StatLength];
        var equipmentItems = this.CurrentEquipment();
        if (equipmentItems == null)
        {
            return null;
        }

        List<uint> itemIds = new();
        foreach (var inventoryItem in equipmentItems)
        {
            this.GetItemStats(ref stats, inventoryItem);
            if (inventoryItem.ItemId > 0)
            {
                itemIds.Add(inventoryItem.ItemId);
            }
        }

        var instance = this.GetGearsetModule();
        var gearsetId = instance->CurrentGearsetIndex;
        var name = this.GetName(gearsetId >= 0 ? instance->GetGearset(gearsetId) : null);
        var player = this.dalamudServices.ClientState.LocalPlayer;
        instance->GetGearset(gearsetId);

        return new Gearset(
            name,
            gearsetId,
            player?.ClassJob.RowId ?? 0,
            player?.Level ?? 0,
            stats,
            equipmentItems.Length >= 12 && equipmentItems[11].ItemId != 0,
            equipmentItems.Length > 0 ? equipmentItems[0].ItemId : 0,
            itemIds);
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

    private void ClientStateOnLogin() => this.UpdateGearsets();

    private void ClientStateOnLogout(int a, int b) => this.Gearsets.Clear();

    private unsafe void GetItemStats(ref uint[] stats, RaptureGearsetModule.GearsetItem* item)
    {
        var materia = new (ushort Id, byte Grade)[5];
        for (var i = 0; i < 5; i++)
        {
            materia[i] = (item->Materia[i], item->MateriaGrades[i]);
        }

        this.GetItemStats(ref stats, item->ItemId, materia);
    }

    private void GetItemStats(ref uint[] stats, InventoryItem item)
    {
        var materia = new (ushort Id, byte Grade)[5];
        for (var i = 0; i < 5; i++)
        {
            materia[i] = (item.Materia[i], item.MateriaGrades[i]);
        }

        var itemId = item.ItemId;
        if ((item.Flags & InventoryItem.ItemFlags.HighQuality) != 0)
        {
            itemId += 1000000U;
        }

        this.GetItemStats(ref stats, itemId, materia);
    }

    private void GetItemStats(ref uint[] stats, uint itemId, (ushort Id, byte Grade)[]? materia = null)
    {
        var flag = itemId > 1000000U;
        if (this.itemsSheet.GetRowOrDefault(itemId % 1000000U) is not { } item || item.LevelItem.ValueNullable is null)
        {
            return;
        }

        var tmpStats = new uint[StatLength];
        foreach (var itemUnkData59Obj in item.BaseParam.Zip(item.BaseParamValue))
        {
            tmpStats[itemUnkData59Obj.First.RowId] += Convert.ToUInt32(itemUnkData59Obj.Second);
        }

        if (flag)
        {
            foreach (var itemUnkData73Obj in item.BaseParamSpecial.Zip(item.BaseParamValueSpecial))
            {
                tmpStats[itemUnkData73Obj.First.RowId] += Convert.ToUInt32(itemUnkData73Obj.Second);
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

                if (this.materiaSheet.GetRowOrDefault(id) is { } m)
                {
                    tmpStats[(byte)m.BaseParam.RowId] += Convert.ToUInt32(m.Value[grade]);
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

            if (this.baseParamSheet.GetRowOrDefault(statId) is not { } baseParam)
            {
                throw new AthavarPluginException($"Fail to find baseParam for statId {statId}");
            }

            var ilvlBase = this.GetColum(this.itemLevelSheet.GetRow(item.LevelItem.RowId), (StatIds)statId);
            if (ilvlBase is null)
            {
                throw new AthavarPluginException($"Fail to find ilvlBase for ilvl {item.LevelItem.RowId} and statId {statId}");
            }

            var maxStat = this.GetMaxStat(baseParam, item, ilvlBase.Value);

            stats[statId] += Math.Min(maxStat, value);
        }
    }

    public ushort? GetColum(ItemLevel sheet, StatIds id)
    {
        return id switch
        {
            StatIds.None => null,
            StatIds.Strength => sheet.Strength,
            StatIds.Dexterity => sheet.Dexterity,
            StatIds.Vitality => sheet.Vitality,
            StatIds.Intelligence => sheet.Intelligence,
            StatIds.Mind => sheet.Mind,
            StatIds.Piety => sheet.Piety,
            StatIds.HP => sheet.HP,
            StatIds.MP => sheet.MP,
            StatIds.TP => sheet.TP,
            StatIds.GP => sheet.GP,
            StatIds.CP => sheet.CP,
            StatIds.PhysicalDamage => sheet.PhysicalDamage,
            StatIds.MagicalDamage => sheet.MagicalDamage,
            StatIds.Delay => sheet.Delay,
            StatIds.AdditionalEffect => sheet.AdditionalEffect,
            StatIds.AttackSpeed => sheet.AttackSpeed,
            StatIds.BlockRate => sheet.BlockRate,
            StatIds.BlockStrength => sheet.BlockStrength,
            StatIds.Tenacity => sheet.Tenacity,
            StatIds.AttackPower => sheet.AttackPower,
            StatIds.Defense => sheet.Defense,
            StatIds.DirectHitRate => sheet.DirectHitRate,
            StatIds.Evasion => sheet.Evasion,
            StatIds.MagicDefense => sheet.MagicDefense,
            StatIds.CriticalHitPower => sheet.CriticalHitPower,
            StatIds.CriticalHitResilience => sheet.CriticalHitResilience,
            StatIds.CriticalHit => sheet.CriticalHit,
            StatIds.CriticalHitEvasion => sheet.CriticalHitEvasion,
            StatIds.SlashingResistance => sheet.SlashingResistance,
            StatIds.PiercingResistance => sheet.PiercingResistance,
            StatIds.BluntResistance => sheet.BluntResistance,
            StatIds.ProjectileResistance => sheet.ProjectileResistance,
            StatIds.AttackMagicPotency => sheet.AttackMagicPotency,
            StatIds.HealingMagicPotency => sheet.HealingMagicPotency,
            StatIds.EnhancementMagicPotency => sheet.EnhancementMagicPotency,
            StatIds.EnfeeblingMagicPotency => sheet.EnfeeblingMagicPotency,
            StatIds.FireResistance => sheet.FireResistance,
            StatIds.IceResistance => sheet.IceResistance,
            StatIds.WindResistance => sheet.WindResistance,
            StatIds.EarthResistance => sheet.EarthResistance,
            StatIds.LightningResistance => sheet.LightningResistance,
            StatIds.WaterResistance => sheet.WaterResistance,
            StatIds.MagicResistance => sheet.MagicResistance,
            StatIds.Determination => sheet.Determination,
            StatIds.SkillSpeed => sheet.SkillSpeed,
            StatIds.SpellSpeed => sheet.SpellSpeed,
            StatIds.Haste => sheet.Haste,
            StatIds.Morale => sheet.Morale,
            StatIds.Enmity => sheet.Enmity,
            StatIds.EnmityReduction => sheet.EnmityReduction,
            StatIds.CarefulDesynthesis => sheet.CarefulDesynthesis,
            StatIds.EXPBonus => sheet.EXPBonus,
            StatIds.Regen => sheet.Regen,
            StatIds.Refresh => sheet.Refresh,
            StatIds.MovementSpeed => sheet.MovementSpeed,
            StatIds.Spikes => sheet.Spikes,
            StatIds.SlowResistance => sheet.SlowResistance,
            StatIds.PetrificationResistance => sheet.PetrificationResistance,
            StatIds.ParalysisResistance => sheet.ParalysisResistance,
            StatIds.SilenceResistance => sheet.SilenceResistance,
            StatIds.BlindResistance => sheet.BlindResistance,
            StatIds.PoisonResistance => sheet.PoisonResistance,
            StatIds.StunResistance => sheet.StunResistance,
            StatIds.SleepResistance => sheet.SleepResistance,
            StatIds.BindResistance => sheet.BindResistance,
            StatIds.HeavyResistance => sheet.HeavyResistance,
            StatIds.DoomResistance => sheet.DoomResistance,
            StatIds.ReducedDurabilityLoss => sheet.ReducedDurabilityLoss,
            StatIds.IncreasedSpiritbondGain => sheet.IncreasedSpiritbondGain,
            StatIds.Craftsmanship => sheet.Craftsmanship,
            StatIds.Control => sheet.Control,
            StatIds.Gathering => sheet.Gathering,
            StatIds.Perception => sheet.Perception,
            StatIds.Unknown73 => sheet.Unknown0,
            _ => throw new ArgumentOutOfRangeException(nameof(id), id, null),
        };
    }

    private uint GetMaxStat(BaseParam param, Item item, uint ilvlBase)
    {
        var equipSlotPercent = this.GetEquipSlotPercent(param, item.EquipSlotCategory.RowId);
        var num = item.BaseParamModifier <= 12 ? param.MeldParam[item.BaseParamModifier] : 0;
        return (uint)Math.Round(ilvlBase * equipSlotPercent * num / 100000.0, MidpointRounding.AwayFromZero);
    }

    private uint GetEquipSlotPercent(BaseParam param, uint category)
    {
        uint equipSlotPercent;
        switch (category)
        {
            case 1:
                equipSlotPercent = param.OneHandWeaponPercent;
                break;
            case 2:
                equipSlotPercent = param.OffHandPercent;
                break;
            case 3:
                equipSlotPercent = param.HeadPercent;
                break;
            case 4:
                equipSlotPercent = param.ChestPercent;
                break;
            case 5:
                equipSlotPercent = param.HandsPercent;
                break;
            case 6:
                equipSlotPercent = param.WaistPercent;
                break;
            case 7:
                equipSlotPercent = param.LegsPercent;
                break;
            case 8:
                equipSlotPercent = param.FeetPercent;
                break;
            case 9:
                equipSlotPercent = param.EarringPercent;
                break;
            case 10:
                equipSlotPercent = param.NecklacePercent;
                break;
            case 11:
                equipSlotPercent = param.BraceletPercent;
                break;
            case 12:
                equipSlotPercent = param.RingPercent;
                break;
            case 13:
                equipSlotPercent = param.TwoHandWeaponPercent;
                break;
            case 14:
                equipSlotPercent = param.UnderArmorPercent;
                break;
            default:
                equipSlotPercent = 0U;
                break;
        }

        return equipSlotPercent;
    }

    private unsafe InventoryManager* GetInventoryManager() => InventoryManager.Instance();

    private unsafe RaptureGearsetModule* GetGearsetModule() => RaptureGearsetModule.Instance();

    private unsafe string GetName(RaptureGearsetModule.GearsetEntry* gearsetEntryPtr) => gearsetEntryPtr is not null ? gearsetEntryPtr->NameString : "<???>";
}