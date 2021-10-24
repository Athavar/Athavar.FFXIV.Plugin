// <copyright file="EquipmentScanner.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Utils
{
    using System;

    using FFXIVClientStructs.FFXIV.Client.Game;

    internal unsafe partial class EquipmentScanner : IDisposable
    {
        private const uint EquipmentContainerSize = 13;

        private InventoryManager* inventoryManager;
        private InventoryContainer* equipmentContainer;
        private InventoryItem* equipmentInventoryItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentScanner"/> class.
        /// </summary>
        public EquipmentScanner()
        {
            DalamudBinding.ClientState.Login += this.ClientStateOnOnLogin;
            if (DalamudBinding.ClientState.IsLoggedIn)
            {
                this.Setup();
            }
        }

        public ushort GetLowestCondition()
        {
            if (!DalamudBinding.ClientState.IsLoggedIn)
            {
                return 0;
            }

            var lowestValue = ushort.MaxValue;
            var inventoryItem = this.equipmentInventoryItem;
            for (var i = 0; i < EquipmentContainerSize; i++, inventoryItem++)
            {
                if (lowestValue > inventoryItem->Condition)
                {
                    lowestValue = inventoryItem->Condition;
                }
            }

            return lowestValue;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            DalamudBinding.ClientState.Login -= this.ClientStateOnOnLogin;
        }

        private void ClientStateOnOnLogin(object? sender, EventArgs e)
        {
            this.Setup();
        }

        private void Setup()
        {
            this.inventoryManager = InventoryManager.Instance();
            this.equipmentContainer = inventoryManager->GetInventoryContainer(InventoryType.EquippedItems);
            this.equipmentInventoryItem = equipmentContainer->GetInventorySlot(0);
        }
    }
}
