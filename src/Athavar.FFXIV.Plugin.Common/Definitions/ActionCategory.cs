// <copyright file="ActionCategory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Definitions;

using System.Text.Json.Serialization;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ActionCategory
{
    Unknown,
    AutoAttack,
    Spell,
    Weaponskill,
    Ability,
    Item,
    DoL_Ability,
    DoH_Ability,
    Event,
    LimitBreak,
    System,
    Artillery,
    Mount,
    Special,
    ItemManipulation,
    AdrenalineRush,
}