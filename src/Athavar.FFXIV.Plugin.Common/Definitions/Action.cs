// <copyright file="Action.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Definitions;

public class Action
{
    public bool IsDamage
    {
        get
        {
            var damage = this.Damage;
            return damage != null && damage.Any();
        }
    }

    public bool IsHeal
    {
        get
        {
            var heal = this.Heal;
            return heal != null && heal.Any();
        }
    }

    public uint Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DamageEntry[]? Damage { get; set; }

    public HealEntry[]? Heal { get; set; }

    public uint GetHealPotency(int targetIndex)
    {
        if (this.Heal is not null && this.Heal.Any(x => x.TargetIndex == targetIndex))
        {
            return this.Heal.First(x => x.TargetIndex == targetIndex).Potency;
        }

        var heal = this.Heal;
        return (heal?.FirstOrDefault(x => !x.TargetIndex.HasValue)?.Potency).GetValueOrDefault();
    }

    public uint GetDamagePotency(int targetIndex, bool isCombo)
    {
        if (this.Damage is not null && this.Damage.Any(x => x.TargetIndex == targetIndex))
        {
            return this.Damage.First(x => x.TargetIndex == targetIndex).Potency;
        }

        var damageEntry = this.Damage?.FirstOrDefault(x => !x.TargetIndex.HasValue);
        if (isCombo && damageEntry is { Combo: { } })
        {
            return damageEntry.Combo.Value;
        }

        return damageEntry?.Potency ?? 0U;
    }
}