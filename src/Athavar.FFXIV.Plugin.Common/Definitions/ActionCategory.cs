// <copyright file="ActionCategory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
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