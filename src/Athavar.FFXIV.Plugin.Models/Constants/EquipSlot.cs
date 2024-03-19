// <copyright file="EquippedItemsSlots.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Constants;

/// <summary>
///     Equip Slot, mostly as defined by the games EquipSlotCategory.
/// </summary>
public enum EquipSlot
{
    Unknown = 0,

    /// <summary>
    ///     Main Weapon.
    /// </summary>
    MainHand = 1,

    /// <summary>
    ///     Offhand.
    /// </summary>
    OffHand = 2,

    /// <summary>
    ///     Head slot.
    /// </summary>
    Head = 3,

    /// <summary>
    ///     Body slot.
    /// </summary>
    Body = 4,

    /// <summary>
    ///     Hand slot.
    /// </summary>
    Hands = 5,

    /// <summary>
    ///     Obsolete. This slot no longer exists.
    /// </summary>
    [Obsolete]
    Waist = 6,

    /// <summary>
    ///     Leg slot.
    /// </summary>
    Legs = 7,

    /// <summary>
    ///     Feet slot.
    /// </summary>
    Feet = 8,

    /// <summary>
    ///     Ear slot.
    /// </summary>
    Ears = 9,

    /// <summary>
    ///     Neck slot.
    /// </summary>
    Neck = 10,

    /// <summary>
    ///     Wrist slot.
    /// </summary>
    Wrists = 11,

    /// <summary>
    ///     Right ring slot.
    /// </summary>
    RFinger = 12,

    /// <summary>
    ///     <see cref="MainHand"/> and <see cref="OffHand"/> slot.
    /// </summary>
    BothHand = 13,

    /// <summary>
    ///     Left ring slot. Not officially existing, means "weapon could be equipped in either hand" for the game.
    /// </summary>
    LFinger = 14,

    /// <summary>
    ///     <see cref="Head"/> and <see cref="Body"/> slot.
    /// </summary>
    HeadBody = 15,

    /// <summary>
    ///     <see cref="Body"/>, <see cref="Hands"/>, <see cref="Legs"/> and <see cref="Feet"/> slot.
    /// </summary>
    BodyHandsLegsFeet = 16,

    /// <summary>
    ///     Soul Crystal slot.
    /// </summary>
    SoulCrystal = 17,

    /// <summary>
    ///     <see cref="Legs"/> and <see cref="Feet"/> slot.
    /// </summary>
    LegsFeet = 18,

    /// <summary>
    ///     <see cref="Head"/>, <see cref="Body"/>, <see cref="Hands"/>, <see cref="Legs"/> and <see cref="Feet"/> slot.
    /// </summary>
    FullBody = 19,

    /// <summary>
    ///     <see cref="Body"/> and <see cref="Hands"/> slot.
    /// </summary>
    BodyHands = 20,

    /// <summary>
    ///     <see cref="Body"/>, <see cref="Legs"/> and <see cref="Feet"/> slot.
    /// </summary>
    BodyLegsFeet = 21,

    /// <summary>
    ///     .
    /// </summary>
    ChestHands = 22,

    /// <summary>
    ///     Uses not slots.
    /// </summary>
    Nothing = 23,

    /// <summary>
    ///     Uses all slots. Not officially existing.
    /// </summary>
    All = 24,
}