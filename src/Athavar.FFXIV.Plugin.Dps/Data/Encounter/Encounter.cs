// <copyright file="Encounter.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Encounter;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Group;

internal sealed class Encounter : BaseEncounter<Combatant>
{
    internal static ObjectTable? ObjectTable;

    private readonly Dictionary<uint, Combatant> combatantMapping = new();

    public Encounter()
    {
    }

    public Encounter(string territoryName, ushort territory, DateTime start)
        : base(territoryName, territory, start)
    {
    }

    public override string Name => $"{this.TerritoryName.AsSpan()} — {this.Title}";

    public TerritoryEncounter? TerritoryEncounter { get; private set; }

    public Combatant? GetCombatant(uint objectId)
    {
        void Add(Combatant c)
        {
            this.Combatants.Add(c);
            this.TerritoryEncounter?.AddCombatant(c);
        }
        
        if (this.combatantMapping.TryGetValue(objectId, out var combatant))
        {
            return combatant;
        }

        if (objectId == uint.MaxValue)
        {
            combatant = new Combatant(this, uint.MaxValue, 0)
                        {
                            Name = "Limit Break",
                            Level = 9999,
                            PartyType = PartyType.Party,
                            Job = Job.LimitBreak,
                        };
            Add(combatant);

            goto end;
        }

        var gameObject = ObjectTable?.SearchById(objectId);

        if (gameObject is PlayerCharacter playerCharacter)
        {
            var name = playerCharacter.Name.ToString();
            var nameSplits = name.Split(" ", 2);

            // create new battle player
            combatant = new Combatant(this, objectId, 0)
                        {
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
        }
        else if (gameObject is BattleNpc battleNpc)
        {
            combatant = this.Combatants.FirstOrDefault(c => c.DataId == battleNpc.DataId);
            if (combatant is not null)
            {
                goto end;
            }

            if (battleNpc.BattleNpcKind == BattleNpcSubKind.Pet && battleNpc.OwnerId != battleNpc.ObjectId)
            {
                // don't have health -> map to owner for effects.
                combatant = this.GetCombatant(battleNpc.OwnerId);
                goto end;
            }

            uint oid = 0;
            uint ownerId = 0;
            var name = battleNpc.Name.ToString();
            var ownerObject = ObjectTable?.SearchById(battleNpc.OwnerId);
            if (ownerObject is not null)
            {
                name = $"{name} ({ownerObject.Name})";
                oid = battleNpc.ObjectId;
                ownerId = battleNpc.OwnerId;
            }

            // create new battle npc
            combatant = new Combatant(this, oid, battleNpc.DataId)
                        {
                            OwnerId = ownerId,
                            Name = name,
                            Name_First = name,
                            Job = battleNpc.BattleNpcKind == BattleNpcSubKind.Chocobo ? Job.Chocobo : (Job)battleNpc.ClassJob.Id,
                            Level = battleNpc.Level,
                            WorldId = 0,
                            WorldName = string.Empty,
                            CurrentWorldId = 0,
                            PartyType = PartyType.None,
                            Kind = battleNpc.BattleNpcKind,
                        };
        }
        else
        {
            // no known type. ignore.
            return null;
        }

        Add(combatant);

        end:

        if (combatant is not null)
        {
            this.combatantMapping.TryAdd(objectId, combatant);
        }

        return combatant;
    }

    public unsafe void UpdateParty(IDalamudServices services)
    {
        var groupManager = GroupManager.Instance();
        var player = services.ClientState.LocalPlayer?.ObjectId;
        var isInAlliance = groupManager->AllianceFlags > 0;

        // PluginLog.LogInformation($"UpdateParty: {string.Join(',', this.Combatants.Select(c => $"{c.ObjectId}:{c.Name} -> {c.Kind.AsText()}"))}");
        var combatants = this.Combatants;
        this.Title = $"{combatants.Where(c => c.Kind == BattleNpcSubKind.Enemy).MaxBy(c => c.DamageTotal)?.Name}";
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

        this.AllyCombatants = combatants.Where(c => !c.IsEnemy() && c.IsActive() && c.IsAlly(this.Filter)).ToList();
    }

    public override bool IsValid() => this.Start != DateTime.MinValue && this.AllyCombatants.Any() && this.Combatants.Any(c => c.Kind == BattleNpcSubKind.Enemy);

    public void CalcAllStats()
    {
        this.CalcStats();
        this.CalcPostStats();
    }

    public void SetTerritoryEncounter(TerritoryEncounter encounter) => this.TerritoryEncounter ??= encounter;
}