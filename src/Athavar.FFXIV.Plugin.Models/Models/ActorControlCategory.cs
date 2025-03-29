// <copyright file="ActorControlCategory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Models;

public enum ActorControlCategory : ushort
{
    Death = 0x6,
    CancelAbility = 0xF,
    GainEffect = 0x14,
    LoseEffect = 0x15,
    UpdateEffect = 0x16,
    TargetIcon = 0x22,
    Tether = 0x23,
    Targetable = 0x36,
    DirectorUpdate = 0x6D,
    SetTargetSign = 0x1F6,
    LimitBreak = 0x1F9,
    HoT = 0x604,
    DoT = 0x605,
}