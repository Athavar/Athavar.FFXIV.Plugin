// <copyright file="IEncounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

using System.Reflection;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Config;

internal abstract class BaseEncounter
{
    [JsonIgnore]
    public static readonly string[] TextTags = new[] { nameof(Duration), nameof(Dps), nameof(Hps), nameof(Deaths), nameof(Kills) }.Select(x => $"[{x.ToLower()}]").ToArray();

    [JsonIgnore]
    private static readonly Dictionary<string, PropertyInfo> Fields = typeof(BaseEncounter).GetProperties().ToDictionary(x => x.Name.ToLower());

    public BaseEncounter()
        : this(string.Empty, 0, DateTime.MinValue)
    {
    }

    public BaseEncounter(string territoryName, ushort territory, DateTime start)
    {
        this.TerritoryName = territoryName;
        this.Territory = territory;
        this.Start = start;
        this.LastEvent = start;
        this.LastDamageEvent = start;
    }

    public virtual TimeSpan Duration => this.End is null ? this.LastEvent - this.Start : this.End.Value - this.Start;

    public abstract string Name { get; }

    public ushort Territory { get; }

    public string TerritoryName { get; }

    public DateTime Start { get; }

    public string? TitleStart => $"{this.Start:T} — {this.Title}";

    public virtual string? Title { get; set; }

    public DateTime LastEvent { get; set; }

    public DateTime LastDamageEvent { get; set; }

    public DateTime? End { get; set; }

    public double Dps { get; protected set; }

    public double Hps { get; protected set; }

    public ulong DamageTotal { get; protected set; }

    public ulong HealingTotal { get; protected set; }

    public ulong DamageTaken { get; protected set; }

    public int Deaths { get; protected set; }

    public int Kills { get; protected set; }

    public PartyType Filter { get; set; }

    public string GetFormattedString(string format, string numberFormat) => TextTagFormatter.TextTagRegex.Replace(format, new TextTagFormatter(this, numberFormat, Fields).Evaluate);

    public abstract IEnumerable<BaseCombatant> GetCombatants();

    public abstract IEnumerable<BaseCombatant> GetAllyCombatants();

    public abstract bool IsValid();
}