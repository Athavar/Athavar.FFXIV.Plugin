// <copyright file="ICommandInterface.Inventory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces;

public partial interface ICommandInterface
{
    /// <summary>
    ///     Check if an Item can be used.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="hq">Indicated if the item has the hq quality.</param>
    /// <returns>returns a bool indication if the item can be used.</returns>
    public bool CanUseItem(uint itemId, bool hq = false);

    /// <summary>
    ///     Use an Item from the inventory.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="hq">Indicated if the item has the hq quality.</param>
    /// <returns>returns a bool indication the item was used successful.</returns>
    public bool UseItem(uint itemId, bool hq = false);

    /// <summary>
    ///     Counts the quantity of an item in the inventory.
    /// </summary>
    /// <param name="itemId">The item id.</param>
    /// <param name="hq">Indicated if the item has the hq quality.</param>
    /// <returns>returns the inventory slot id.</returns>
    public uint CountItem(uint itemId, bool hq = false);

    /// <summary>
    ///     Counts the number of free slots in the inventory.
    /// </summary>
    /// <returns>returns the inventory slot id.</returns>
    public uint FreeInventorySlots();

    /// <summary>
    ///     Gets a value indicating whether any of the player's worn equipment is broken.
    /// </summary>
    /// <returns>A value indicating whether any of the player's worn equipment is broken.</returns>
    public bool NeedsRepair();

    /// <summary>
    ///     Gets a value indicating whether any of the player's worn equipment can have materia extracted.
    /// </summary>
    /// <param name="within">Return false if the next highest spiritbond is >= this value.</param>
    /// <returns>A value indicating whether any of the player's worn equipment can have materia extracted.</returns>
    public bool CanExtractMateria(float within = 100);

    /// <summary>
    ///     Gets a value indicating whether the required crafting stats have been met.
    /// </summary>
    /// <param name="craftsmanship">Craftsmanship.</param>
    /// <param name="control">Control.</param>
    /// <param name="cp">Crafting points.</param>
    /// <returns>A value indicating whether the required crafting stats have been met.</returns>
    public bool HasStats(uint craftsmanship, uint control, uint cp);
}