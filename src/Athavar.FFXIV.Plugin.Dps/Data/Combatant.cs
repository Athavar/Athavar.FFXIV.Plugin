// <copyright file="Combatant.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using System.Reflection;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Game.ClientState.Objects.Enums;

internal class Combatant
{
    [JsonIgnore]
    public static readonly string[] TextTags = typeof(Combatant).GetProperties().Select(x => $"[{x.Name.ToLower()}]").ToArray();

    public readonly List<CombatEvent.StatusEffect> StatusList = new();

    public uint ObjectId = 0;

    public uint DataId = 0;

    public uint OwnerId = 0;

    [JsonIgnore]
    private static readonly Dictionary<string, PropertyInfo> Fields = typeof(Combatant).GetProperties().ToDictionary(x => x.Name.ToLower());

    private readonly Encounter encounter;

    internal Combatant(Encounter encounter) => this.encounter = encounter;

    public double Dps { get; private set; }

    public double Hps { get; private set; }

    public double OverHealPct { get; private set; }

    public double DamagePct { get; private set; }

    public ulong EffectiveHealing { get; private set; }

    public string Name { get; init; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string Name_First { get; init; } = string.Empty;

    // ReSharper disable once InconsistentNaming
    public string Name_Last { get; init; } = string.Empty;

    public Job Job { get; set; }

    public int Level { get; set; }

    public uint CurrentWorldId { get; set; }

    public uint WorldId { get; init; }

    public string WorldName { get; init; } = string.Empty;

    public PartyType PartyType { get; set; } = PartyType.None;

    public BattleNpcSubKind Kind { get; init; }

    public ushort Deaths { get; set; } = 0;

    public ushort Kills { get; set; } = 0;

    public ulong HealingTotal { get; set; } = 0;

    public ulong HealingTaken { get; set; } = 0;

    public ulong OverHealTotal { get; set; } = 0;

    public ulong DamageTotal { get; set; } = 0;

    public ulong DamageTaken { get; set; } = 0;

    public string Rank { get; set; } = string.Empty;

    public override string ToString() => this.ToString(this.ObjectId == 0 ? this.DataId : this.ObjectId);

    public string GetFormattedString(string format, string numberFormat) => TextTagFormatter.TextTagRegex.Replace(format, new TextTagFormatter(this, numberFormat, Fields).Evaluate);

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

    public void CalcPreCombatantStats()
    {
        this.Dps = this.encounter.Duration.TotalSeconds == 0 ? this.DamageTotal : Math.Round(this.DamageTotal / this.encounter.Duration.TotalSeconds, 2);
        this.Hps = this.encounter.Duration.TotalSeconds == 0 ? this.HealingTotal : Math.Round(this.HealingTotal / this.encounter.Duration.TotalSeconds, 2);
        this.EffectiveHealing = this.HealingTotal - this.OverHealTotal;
        this.OverHealPct = this.HealingTotal == 0 ? 0 : Math.Round(((double)this.OverHealTotal / this.HealingTotal) * 100, 2);
    }

    public void CalcCombatantStats() => this.DamagePct = this.DamageTotal == 0 ? 0 : Math.Round(((double)this.DamageTotal / this.encounter.DamageTotal) * 100, 2);
}