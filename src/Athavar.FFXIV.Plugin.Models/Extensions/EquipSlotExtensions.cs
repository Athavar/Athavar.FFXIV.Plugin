// <copyright file="EquipSlotExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Extensions;

using Athavar.FFXIV.Plugin.Models.Constants;

public static class EquipSlotExtensions
{
    /// <summary>
    ///     Convert the integer to the <see cref="EquipSlot"/> it is used to represent in most model code.
    /// </summary>
    /// <param name="value">The integer value.</param>
    /// <returns>The <see cref="EquipSlot"/>.</returns>
    public static EquipSlot ToEquipSlot(this uint value)
        => value switch
        {
            0 => EquipSlot.Head,
            1 => EquipSlot.Body,
            2 => EquipSlot.Hands,
            3 => EquipSlot.Legs,
            4 => EquipSlot.Feet,
            5 => EquipSlot.Ears,
            6 => EquipSlot.Neck,
            7 => EquipSlot.Wrists,
            8 => EquipSlot.RFinger,
            9 => EquipSlot.LFinger,
            10 => EquipSlot.MainHand,
            11 => EquipSlot.OffHand,
            _ => EquipSlot.Unknown,
        };

    /// <summary>
    ///     Convert an <see cref="EquipSlot"/> to the index it is used at in most model code.
    /// </summary>
    /// <param name="slot">The <see cref="EquipSlot"/>.</param>
    /// <returns>The index.</returns>
    public static uint ToIndex(this EquipSlot slot)
        => slot switch
        {
            EquipSlot.Head => 0,
            EquipSlot.Body => 1,
            EquipSlot.Hands => 2,
            EquipSlot.Legs => 3,
            EquipSlot.Feet => 4,
            EquipSlot.Ears => 5,
            EquipSlot.Neck => 6,
            EquipSlot.Wrists => 7,
            EquipSlot.RFinger => 8,
            EquipSlot.LFinger => 9,
            EquipSlot.MainHand => 10,
            EquipSlot.OffHand => 11,
            _ => uint.MaxValue,
        };
}