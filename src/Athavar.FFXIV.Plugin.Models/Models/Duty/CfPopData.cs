// <copyright file="CfPopData.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Duty;

using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Explicit)]
public struct CfPopData
{
    [FieldOffset(0)]
    private byte notifyType;

    // 1 byte

    [FieldOffset(0x08)]
    private byte cc0;

    [FieldOffset(0x09)]
    private byte cc1;

    [FieldOffset(0x0A)]
    private byte cc2;

    [FieldOffset(0x0B)]
    private byte cc3;

    [FieldOffset(0x0C)]
    private byte cc4;

    [FieldOffset(0x21)]
    private byte tankCount;

    [FieldOffset(0x23)]
    private byte healerCount;

    [FieldOffset(0x25)]
    private byte damageDealerCount;

    [FieldOffset(0x27)]
    private byte otherRolesCount;

    [field: FieldOffset(0x02)]
    public ushort ContentRouletteId { get; }

    [field: FieldOffset(0x1C)]
    public ushort ContentFinderConditionId { get; }

    public bool IsJoinInProgressEnabled => (this.cc0 & 0x02) != 0;

    public bool IsNotQueued => (this.cc0 & 0x04) != 0;

    public bool IsPartyLead => (this.cc0 & 0x10) != 0;

    public bool HasContentFinderCondition => (this.cc0 & 0x10) != 0;

    // ? need more data. Queued duty?
    public bool HasQueued => (this.cc0 & 0x40) != 0;

    // ? need more data.
    public bool JoinInProgress => (this.cc0 & 0x80) != 0;

    // ? need more data.
    public bool IsRoulette => (this.cc1 & 0x01) != 0;

    public bool IsRoleInNeed => (this.cc1 & 0x02) != 0;

    // ? need more data.
    public bool IsSoloQueue => (this.cc1 & 0x04) != 0;

    public bool IsUnrestrictedPartyEnabled => (this.cc1 & 0x20) != 0;

    public bool IsMinimalILEnabled => (this.cc1 & 0x40) != 0;

    public bool IsGreedOnlyEnabled => (this.cc1 & 0x80) != 0;

    public bool IsLootMasterEnabled => (this.cc2 & 0x01) != 0;

    public bool Unk2X02 => (this.cc2 & 0x02) != 0;

    // ? Need more Data to check
    public bool IsMentor => (this.cc2 & 0x04) != 0;

    // ? flag was only set for mentor roulette
    public bool IsMentorRoulette => (this.cc2 & 0x08) != 0;

    public bool IsCrossWorldParty => (this.cc2 & 0x10) != 0;

    public bool IsLevelSyncEnabled => (this.cc2 & 0x20) != 0;

    public bool IsSilenceEchoEnabled => (this.cc3 & 0x10) != 0;

    public bool IsLimitedLevelingRoulette => (this.cc3 & 0x40) != 0;

    public bool IsExplorerModeEnabled => (this.cc4 & 0x01) != 0;

    public int Tanks => this.tankCount;

    public int Healer => this.healerCount;

    public int DamageDealer => this.damageDealerCount;

    public int OtherRole => this.otherRolesCount;

    public int PlayerCount => this.tankCount + this.healerCount + this.damageDealerCount + this.otherRolesCount;

    public ContentCondition GetActiveContentCondition()
    {
        ContentCondition result = 0;
        if (this.IsJoinInProgressEnabled)
        {
            result |= ContentCondition.JoinInProgress;
        }

        if (this.IsUnrestrictedPartyEnabled)
        {
            result |= ContentCondition.UnrestrictedParty;
        }

        if (this.IsMinimalILEnabled)
        {
            result |= ContentCondition.MinimalIL;
        }

        if (this.IsLevelSyncEnabled)
        {
            result |= ContentCondition.LevelSync;
        }

        if (this.IsSilenceEchoEnabled)
        {
            result |= ContentCondition.SilenceEcho;
        }

        if (this.IsLimitedLevelingRoulette)
        {
            result |= ContentCondition.LimitedLevelingRoulette;
        }

        if (this.IsExplorerModeEnabled)
        {
            result |= ContentCondition.ExplorerMode;
        }

        return result;
    }

    public LootRule GetLootRule() => this.IsLootMasterEnabled ? LootRule.LootMaster : this.IsGreedOnlyEnabled ? LootRule.GreedOnly : LootRule.Default;

    public override string ToString()
    {
        var selCond = this.GetActiveContentCondition();
        var selCondString = string.Join("|", Enum.GetValues<ContentCondition>().Where(r => (selCond & r) == r).Select(r => r.ToString()));
        return $"NotifyType:{this.notifyType} ConditionId:{this.ContentFinderConditionId} T:{this.tankCount} H:{this.healerCount} D:{this.damageDealerCount} O:{this.otherRolesCount} SelectedConditions:{selCondString}";
    }
}