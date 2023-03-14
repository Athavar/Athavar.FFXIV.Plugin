// <copyright file="Gearset.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Utils;

using Athavar.FFXIV.Plugin.Common.Utils.Constants;
using Athavar.FFXIV.Plugin.Config;

public sealed class Gearset
{
    private readonly uint[] stats;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Gearset" /> class.
    /// </summary>
    /// <param name="name">THe name of the gearset.</param>
    /// <param name="id">The id of the gearset.</param>
    /// <param name="jobClass">The jobclass of the gearset.</param>
    /// <param name="level">The jobClass level.</param>
    /// <param name="stats">The stats.</param>
    /// <param name="hasSoulStone">The soulstone check.</param>
    public Gearset(string name, byte id, uint jobClass, byte level, uint[] stats, bool hasSoulStone)
    {
        this.Name = name;
        this.Id = id;
        this.JobClass = (Job)jobClass;
        this.JobLevel = level;
        this.stats = stats;
        this.HasSoulStone = hasSoulStone;
    }

    /// <summary>
    ///     Gets the name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    ///     Gets the id.
    /// </summary>
    public byte Id { get; }

    /// <summary>
    ///     Gets the jobclass.
    /// </summary>
    public Job JobClass { get; }

    /// <summary>
    ///     Gets the jobclass level.
    /// </summary>
    public byte JobLevel { get; }

    /// <summary>
    ///     Gets a value indicating whether the gearset has a soulstone.
    /// </summary>
    public bool HasSoulStone { get; }

#pragma warning disable SA1516
#pragma warning disable SA1600
    public uint Strength => this.stats[1U];

    public uint Dexterity => this.stats[2U];

    public uint Vitality => this.stats[3U];

    public uint Intelligence => this.stats[4U];

    public uint Mind => this.stats[5U];

    public uint Piety => this.stats[6U];

    public uint HP => this.stats[7U];

    public uint MP => this.stats[8U];

    public uint TP => this.stats[9U];

    public uint GP => this.stats[10U];

    public uint CP => this.stats[(ushort)StatIds.CP] + 180;

    public uint PhysicalDamage => this.stats[12U];

    public uint MagicalDamage => this.stats[13U];

    public uint Delay => this.stats[14U];

    public uint AdditionalEffect => this.stats[15U];

    public uint AttackSpeed => this.stats[16U];

    public uint BlockRate => this.stats[17U];

    public uint BlockStrength => this.stats[18U];

    public uint Tenacity => this.stats[19U];

    public uint AttackPower => this.stats[20U];

    public uint Defense => this.stats[21U];

    public uint DirectHitRate => this.stats[22U];

    public uint Evasion => this.stats[23U];

    public uint MagicDefense => this.stats[24U];

    public uint CriticalHitPower => this.stats[25U];

    public uint CriticalHitResilience => this.stats[26U];

    public uint CriticalHit => this.stats[27U];

    public uint CriticalHitEvasion => this.stats[28U];

    public uint SlashingResistance => this.stats[29U];

    public uint PiercingResistance => this.stats[30U];

    public uint BluntResistance => this.stats[31U];

    public uint ProjectileResistance => this.stats[32U];

    public uint AttackMagicPotency => this.stats[33U];

    public uint HealingMagicPotency => this.stats[34U];

    public uint EnhancementMagicPotency => this.stats[35U];

    public uint EnfeeblingMagicPotency => this.stats[36U];

    public uint FireResistance => this.stats[37U];

    public uint IceResistance => this.stats[38U];

    public uint WindResistance => this.stats[39U];

    public uint EarthResistance => this.stats[40U];

    public uint LightningResistance => this.stats[41U];

    public uint WaterResistance => this.stats[42U];

    public uint MagicResistance => this.stats[43U];

    public uint Determination => this.stats[44U];

    public uint SkillSpeed => this.stats[45U];

    public uint SpellSpeed => this.stats[46U];

    public uint Haste => this.stats[47U];

    public uint Morale => this.stats[48U];

    public uint Enmity => this.stats[49U];

    public uint EnmityReduction => this.stats[50U];

    public uint CarefulDesynthesis => this.stats[51U];

    public uint EXPBonus => this.stats[52U];

    public uint Regen => this.stats[53U];

    public uint Refresh => this.stats[54U];

    public uint MovementSpeed => this.stats[55U];

    public uint Spikes => this.stats[56U];

    public uint SlowResistance => this.stats[57U];

    public uint PetrificationResistance => this.stats[58U];

    public uint ParalysisResistance => this.stats[59U];

    public uint SilenceResistance => this.stats[60U];

    public uint BlindResistance => this.stats[61U];

    public uint PoisonResistance => this.stats[62U];

    public uint StunResistance => this.stats[63U];

    public uint SleepResistance => this.stats[64U];

    public uint BindResistance => this.stats[65U];

    public uint HeavyResistance => this.stats[66U];

    public uint DoomResistance => this.stats[67U];

    public uint ReducedDurabilityLoss => this.stats[68U];

    public uint IncreasedSpiritbondGain => this.stats[69U];

    public uint Craftsmanship => this.stats[(ushort)StatIds.Craftsmanship];

    public uint Control => this.stats[(ushort)StatIds.Control];

    public uint Gathering => this.stats[72U];

    public uint Perception => this.stats[73U];

    public uint Unknown73 => this.stats[74U];
#pragma warning restore SA1600
#pragma warning restore SA1516
}