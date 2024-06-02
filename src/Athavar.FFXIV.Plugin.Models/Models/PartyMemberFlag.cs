// <copyright file="PartyMemberFlag.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Models;

[Flags]
public enum PartyMemberFlag
{
    None = 0x00,
    Fill = 0x01, // Contains player
    Debuff = 0x02, // alliance only
    Reserved = 0x04, // slot is reserved aand to be filled
    Synced = 0x08,
}