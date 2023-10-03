// <copyright file="CommandInterface.Inventory.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using Athavar.FFXIV.Plugin.Config;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

internal sealed partial class CommandInterface
{
    private readonly InventoryType[] playerInventories = new InventoryType[5]
    {
        InventoryType.Inventory1,
        InventoryType.Inventory2,
        InventoryType.Inventory3,
        InventoryType.Inventory4,
        InventoryType.Crystals,
    };

    /// <inheritdoc/>
    public unsafe bool CanUseItem(uint itemId, bool hq = false)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var actionId = itemId + (hq ? 1000000U : 0U);
        return ActionManager.Instance()->GetActionStatus(ActionType.Item, actionId, Constants.PlayerId, true, true, null) == 0U;
    }

    /// <inheritdoc/>
    public unsafe bool UseItem(uint itemId, bool hq = false)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var actionId = itemId + (hq ? 1000000U : 0U);
        return this.CanUseItem(itemId, hq) && ActionManager.Instance()->UseAction(ActionType.Item, actionId, Constants.PlayerId, ushort.MaxValue, 0U, 0U, null);
    }

    /// <inheritdoc/>
    public unsafe uint CountItem(uint itemId, bool hq = false)
    {
        if (!this.IsLoggedIn())
        {
            return 0;
        }

        var num = 0;
        foreach (var playerInventory in this.playerInventories)
        {
            num += InventoryManager.Instance()->GetItemCountInContainer(itemId, playerInventory, hq);
        }

        return (uint)num;
    }

    /// <inheritdoc/>
    public unsafe uint FreeInventorySlots()
    {
        if (!this.IsLoggedIn())
        {
            return 0;
        }

        var num = 0;
        for (var index = 0; index < this.playerInventories.Length - 1; index++)
        {
            var playerInventory = InventoryManager.Instance()->GetInventoryContainer(this.playerInventories[index]);
            for (var slotIndex = 0; slotIndex < playerInventory->Size; slotIndex++)
            {
                var slot = playerInventory->GetInventorySlot(slotIndex);
                if (slot->ItemID == 0)
                {
                    num++;
                }
            }
        }

        return (uint)num;
    }

    /// <inheritdoc/>
    public unsafe bool NeedsRepair()
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var im = InventoryManager.Instance();
        if (im == null)
        {
            this.logger.Error("InventoryManager was null");
            return false;
        }

        var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (equipped == null)
        {
            this.logger.Error("InventoryContainer was null");
            return false;
        }

        if (equipped->Loaded == 0)
        {
            this.logger.Error("InventoryContainer is not loaded");
            return false;
        }

        for (var i = 0; i < equipped->Size; i++)
        {
            var item = equipped->GetInventorySlot(i);
            if (item == null)
            {
                continue;
            }

            if (item->Condition == 0)
            {
                return true;
            }
        }

        return false;
    }

    /// <inheritdoc/>
    public unsafe bool CanExtractMateria(float within = 100)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var im = InventoryManager.Instance();
        if (im == null)
        {
            this.logger.Error("InventoryManager was null");
            return false;
        }

        var equipped = im->GetInventoryContainer(InventoryType.EquippedItems);
        if (equipped == null)
        {
            this.logger.Error("InventoryContainer was null");
            return false;
        }

        if (equipped->Loaded == 0)
        {
            this.logger.Error("InventoryContainer is not loaded");
            return false;
        }

        var nextHighest = 0f;
        var canExtract = false;
        var allExtract = true;
        for (var i = 0; i < equipped->Size; i++)
        {
            var item = equipped->GetInventorySlot(i);
            if (item == null)
            {
                continue;
            }

            var spiritbond = item->Spiritbond / 100;
            if (spiritbond == 100f)
            {
                canExtract = true;
            }
            else
            {
                allExtract = false;
                nextHighest = Math.Max(spiritbond, nextHighest);
            }
        }

        if (allExtract)
        {
            this.logger.Debug("All items are spiritbound, pausing");
            return true;
        }

        if (canExtract)
        {
            // Don't wait, extract immediately
            if (within == 100)
            {
                this.logger.Debug("An item is spiritbound, pausing");
                return true;
            }

            // Keep going if the next highest spiritbonded item is within the allowed range
            // i.e. 100 and 99, do another craft to finish the 99.
            if (nextHighest >= within)
            {
                this.logger.Debug($"The next highest spiritbond is above ({nextHighest} >= {within}), keep going");
                return false;
            }

            this.logger.Debug($"The next highest spiritbond is below ({nextHighest} < {within}), pausing");
            return true;
        }

        return false;
    }

    /// <inheritdoc/>
    public unsafe bool HasStats(uint craftsmanship, uint control, uint cp)
    {
        if (!this.IsLoggedIn())
        {
            return false;
        }

        var uiState = UIState.Instance();
        if (uiState == null)
        {
            this.logger.Error("UIState is null");
            return false;
        }

        var hasStats =
            uiState->PlayerState.Attributes[70] >= craftsmanship &&
            uiState->PlayerState.Attributes[71] >= control &&
            uiState->PlayerState.Attributes[11] >= cp;

        return hasStats;
    }
}