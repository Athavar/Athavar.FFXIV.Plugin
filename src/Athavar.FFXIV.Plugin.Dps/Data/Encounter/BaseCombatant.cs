// <copyright file="ICombatant.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

using System.Reflection;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Game.ClientState.Objects.Enums;

internal abstract class BaseCombatant
{
    [JsonIgnore]
    public static readonly string[] TextTags = new[]
                                               {
                                                   nameof(Dps),
                                                   nameof(Hps),
                                                   nameof(Name),
                                                   nameof(Name_First),
                                                   nameof(Name_Last),
                                                   nameof(Job),
                                                   nameof(CurrentWorldId),
                                                   nameof(WorldId),
                                                   nameof(PartyType),
                                                   nameof(Kind),
                                                   nameof(Deaths),
                                                   nameof(Kills),
                                                   nameof(HealingTotal),
                                                   nameof(HealingTaken),
                                                   nameof(OverHealTotal),
                                                   nameof(OverHealPct),
                                                   nameof(EffectiveHealing),
                                                   nameof(DamageTotal),
                                                   nameof(DamageTaken),
                                                   nameof(DamagePct),
                                                   nameof(Rank),
                                               }.Select(x => $"[{x.ToLower()}]").ToArray();

    [JsonIgnore]
    private static readonly Dictionary<string, PropertyInfo> Fields = typeof(BaseCombatant).GetProperties().ToDictionary(x => x.Name.ToLower());

    public uint ObjectId { get; init; }

    public uint DataId { get; init; }

    public double Dps { get; protected set; }

    public double Hps { get; protected set; }

    public double OverHealPct { get; protected set; }

    public double DamagePct { get; protected set; }

    public ulong EffectiveHealing { get; protected set; }

    public string Name { get; init; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string Name_First { get; init; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string Name_Last { get; init; } = string.Empty;

    public Job Job { get; init; } = Job.Adventurer;

    public int Level { get; init; }

    public uint CurrentWorldId { get; init; }

    public uint WorldId { get; init; }

    public string WorldName { get; init; } = string.Empty;

    public BattleNpcSubKind Kind { get; init; }

    public ushort Deaths { get; set; }

    public ushort Kills { get; set; }

    public ulong HealingTotal { get; protected set; }

    public ulong HealingTaken { get; protected set; }

    public ulong OverHealTotal { get; protected set; }

    public ulong DamageTotal { get; protected set; }

    public ulong DamageTaken { get; protected set; }

    public PartyType PartyType { get; set; }

    public uint OwnerId { get; init; }

    public string Rank { get; set; } = string.Empty;

    public string GetFormattedString(string format, string numberFormat) => TextTagFormatter.TextTagRegex.Replace(format, new TextTagFormatter(this, numberFormat, Fields).Evaluate);

    public override string ToString() => this.ToString(this.ObjectId == 0 ? this.DataId : this.ObjectId);

    public string ToString(uint objectId) => $"'{this.Name}' <{objectId:X}> {this.Kind.AsText()}";

    public float GetMeterData(MeterDataType type)
        => type switch
           {
               MeterDataType.Damage => this.DamageTotal,
               MeterDataType.Healing => this.HealingTotal,
               MeterDataType.EffectiveHealing => this.EffectiveHealing,
               MeterDataType.DamageTaken => this.DamageTaken,
               _ => 0,
           };

    public abstract void CalcStats();

    public abstract void PostCalcStats();

    public bool IsEnemy() => this.Kind is BattleNpcSubKind.Enemy or (BattleNpcSubKind)1;

    public bool IsAlly(PartyType filter) => this.PartyType <= filter;

    public bool IsActive() => this is not { DamageTaken: 0, HealingTotal: 0, DamageTotal: 0 };
}