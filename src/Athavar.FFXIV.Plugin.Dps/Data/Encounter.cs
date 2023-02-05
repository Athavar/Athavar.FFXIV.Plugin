// <copyright file="Encounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data;

using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Logging;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

internal class Encounter
{
    [JsonIgnore]
    public static readonly string[] TextTags = typeof(Encounter).GetProperties().Select(x => $"[{x.Name.ToLower()}]").ToArray();

    public readonly List<Combatant> Combatants = new();

    public readonly ushort Territory;

    public readonly string TerritoryName;

    public string? Title;

    public List<Combatant> AllyCombatants = new();

    public DateTime Start = DateTime.MinValue;

    public DateTime LastEvent;

    public DateTime LastDamageEvent;

    public DateTime? End;

    internal static ObjectTable objectTable;

    [JsonIgnore]
    private static readonly Dictionary<string, PropertyInfo> Fields = typeof(Encounter).GetProperties().ToDictionary(x => x.Name.ToLower());

    private readonly Dictionary<uint, Combatant> combatantMapping = new();

    public Encounter()
        : this(string.Empty, 0)
    {
    }

    public Encounter(string territoryName, ushort territory)
    {
        this.TerritoryName = territoryName;
        this.Territory = territory;
    }

    public TimeSpan Duration => this.End is null ? this.LastEvent - this.Start : this.End.Value - this.Start;

    public double Dps { get; private set; }

    public double Hps { get; private set; }

    public ulong DamageTotal { get; private set; }

    public int Deaths { get; private set; }

    public int Kills { get; private set; }

    public string GetFormattedString(string format, string numberFormat) => TextTagFormatter.TextTagRegex.Replace(format, new TextTagFormatter(this, numberFormat, Fields).Evaluate);

    public Combatant? GetCombatant(uint objectId)
    {
        if (this.combatantMapping.TryGetValue(objectId, out var combatant))
        {
            return combatant;
        }

        if (objectId == uint.MaxValue)
        {
            combatant = new Combatant(this)
            {
                Name = "Limit Break",
                Level = 9999,
                PartyType = PartyType.Party,
            };
            this.Combatants.Add(combatant);

            goto end;
        }

        var gameObject = objectTable.SearchById(objectId);

        if (gameObject is PlayerCharacter playerCharacter)
        {
            var name = playerCharacter.Name.ToString();
            var nameSplits = name.Split(" ", 2);

            // create new battle player
            combatant = new Combatant(this)
            {
                ObjectId = objectId,
                Name = name,
                Name_First = nameSplits[0],
                Name_Last = nameSplits.Length == 2 ? nameSplits[1] : string.Empty,
                Job = (Job)playerCharacter.ClassJob.Id,
                Level = playerCharacter.Level,
                WorldId = playerCharacter.HomeWorld.Id,
                WorldName = playerCharacter.HomeWorld.GameData?.Name.ToDalamudString().ToString() ?? "<Unknown>",
                CurrentWorldId = playerCharacter.CurrentWorld.Id,
                PartyType = PartyType.None,
                Kind = BattleNpcSubKind.None,
            };
            this.Combatants.Add(combatant);
        }
        else if (gameObject is BattleNpc battleNpc)
        {
            combatant = this.Combatants.FirstOrDefault(c => c.DataId == battleNpc.DataId);
            if (combatant is null)
            {
                if (battleNpc.BattleNpcKind == BattleNpcSubKind.Pet && battleNpc.OwnerId != battleNpc.ObjectId)
                {
                    // don't have health -> map to owner for effects.
                    combatant = this.GetCombatant(battleNpc.OwnerId);
                    goto end;
                }

                uint oid = 0;
                uint ownerId = 0;
                var name = battleNpc.Name.ToString();
                var ownerObject = objectTable.SearchById(battleNpc.OwnerId);
                if (ownerObject is not null)
                {
                    name = $"{name} ({ownerObject.Name})";
                    oid = battleNpc.ObjectId;
                    ownerId = battleNpc.OwnerId;
                }

                // create new battle npc
                combatant = new Combatant(this)
                {
                    ObjectId = oid,
                    DataId = battleNpc.DataId,
                    OwnerId = ownerId,
                    Name = name,
                    Name_First = name,
                    Job = (Job)battleNpc.ClassJob.Id,
                    Level = battleNpc.Level,
                    WorldId = 0,
                    WorldName = string.Empty,
                    CurrentWorldId = 0,
                    PartyType = PartyType.None,
                    Kind = battleNpc.BattleNpcKind,
                };

                this.Combatants.Add(combatant);
            }
        }

        end:

        if (combatant is not null)
        {
            this.combatantMapping.TryAdd(objectId, combatant);
        }

        return combatant;
    }

    public unsafe void UpdateParty(DpsConfiguration configuration, IDalamudServices services)
    {
        var groupManager = GroupManager.Instance();
        var player = services.ClientState.LocalPlayer?.ObjectId;
        var isInAlliance = groupManager->AllianceFlags > 0;

        // PluginLog.LogInformation($"UpdateParty: {string.Join(',', this.Combatants.Select(c => $"{c.ObjectId}:{c.Name} -> {c.Kind.AsText()}"))}");
        var combatants = this.Combatants;
        this.Title = $"{this.TerritoryName} - {combatants.Where(c => c.Kind == BattleNpcSubKind.Enemy).MaxBy(c => c.DamageTotal)?.Name}";
        foreach (var combatant in combatants.Where(c => c.ObjectId != 0))
        {
            if (combatant.ObjectId == player)
            {
                combatant.PartyType = PartyType.Self;
                continue;
            }

            combatant.PartyType = groupManager->IsObjectIDInParty(combatant.ObjectId) ? PartyType.Party : isInAlliance && groupManager->IsObjectIDInAlliance(combatant.ObjectId) ? PartyType.Alliance : PartyType.None;
        }

        // match chocobo ownership with owner. 
        foreach (var chocobo in combatants.Where(c => c.Kind == BattleNpcSubKind.Chocobo))
        {
            chocobo.PartyType = combatants.Find(c => c.ObjectId == chocobo.OwnerId)?.PartyType ?? PartyType.None;
        }

        var filter = configuration.PartyFilter;
        this.AllyCombatants = combatants
           .Where(c => c is not { DamageTaken: 0, HealingTotal: 0, DamageTotal: 0 } && c.Kind != BattleNpcSubKind.Enemy)
           .Where(c => c.PartyType <= filter)
           .ToList();

        PluginLog.Information(string.Join(',', this.Combatants.Select(c => $"{c.Name} -> {c.PartyType}")));
    }

    public void CalcEncounterStats()
    {
        var combatants = this.AllyCombatants;
        double dps = 0;
        double hps = 0;
        ulong damageTotal = 0;
        var deaths = 0;
        var kills = 0;
        foreach (var combatant in CollectionsMarshal.AsSpan(combatants))
        {
            combatant.CalcPreCombatantStats();
            dps += combatant.Dps;
            hps += combatant.Hps;
            damageTotal += combatant.DamageTotal;
            deaths += combatant.Deaths;
            kills += combatant.Kills;
        }

        this.Dps = Math.Round(dps, 2);
        this.Hps = Math.Round(hps, 2);
        this.DamageTotal = damageTotal;
        this.Deaths = deaths;
        this.Kills = kills;
        foreach (var combatant in CollectionsMarshal.AsSpan(combatants))
        {
            combatant.CalcCombatantStats();
        }
    }

    public bool IsValid() => this.Start != DateTime.MinValue && this.AllyCombatants.Any() && this.Combatants.Any(c => c.Kind == BattleNpcSubKind.Enemy);
}