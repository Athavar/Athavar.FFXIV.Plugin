// <copyright file="EquipmentScanner.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Utils;

using System;
using Athavar.FFXIV.Plugin.Manager.Interface;
using FFXIVClientStructs.FFXIV.Client.Game;

internal unsafe class EquipmentScanner : IDisposable
{
    private const uint EquipmentContainerSize = 13;
    private readonly IDalamudServices dalamudServices;

    private InventoryManager* inventoryManager;
    private InventoryContainer* equipmentContainer;
    private InventoryItem* equipmentInventoryItem;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EquipmentScanner" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public EquipmentScanner(IDalamudServices dalamudServices)
    {
        this.dalamudServices = dalamudServices;

        dalamudServices.ClientState.Login += this.ClientStateOnOnLogin;
        if (dalamudServices.ClientState.IsLoggedIn)
        {
            this.Setup();
        }
    }

    public ushort GetLowestCondition()
    {
        if (!this.dalamudServices.ClientState.IsLoggedIn)
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

    /// <inheritdoc />
    public void Dispose() => this.dalamudServices.ClientState.Login -= this.ClientStateOnOnLogin;

    private void ClientStateOnOnLogin(object? sender, EventArgs e) => this.Setup();

    private void Setup()
    {
        this.inventoryManager = InventoryManager.Instance();
        this.equipmentContainer = this.inventoryManager->GetInventoryContainer(InventoryType.EquippedItems);
        this.equipmentInventoryItem = this.equipmentContainer->GetInventorySlot(0);
    }
}