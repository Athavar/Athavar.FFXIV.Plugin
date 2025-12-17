// <copyright file="ItemLevelExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Extension;

using Athavar.FFXIV.Plugin.Models.Constants;
using Lumina.Excel.Sheets;

public static class ItemLevelExtensions
{
    public static ushort? GetColum(this ItemLevel sheet, StatIds id)
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
}