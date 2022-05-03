// <copyright file="ItemInspectorModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.ItemInspector;

using System;
using System.Collections.Generic;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Game.Command;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game;

internal class ItemInspectorModule : IModule, IDisposable
{
    private const string ModuleName = "ItemInspector";
    private readonly ItemInspectorTab tab;

    private readonly IDalamudServices dalamudServices;

    private readonly Dictionary<Inventory, InventoryType[]> InventoryMapping =
        new()
        {
            {
                Inventory.Player, new[] { InventoryType.Inventory1, InventoryType.Inventory2, InventoryType.Inventory3, InventoryType.Inventory4 }
            },
            {
                Inventory.Retainer, new[] { InventoryType.RetainerPage1, InventoryType.RetainerPage2, InventoryType.RetainerPage3, InventoryType.RetainerPage4 }
            },
        };

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemInspectorModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="IModuleManager" /> added by DI.</param>
    /// <param name="itemInspectorTab"><see cref="ItemInspectorTab" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public ItemInspectorModule(IModuleManager moduleManager, ItemInspectorTab itemInspectorTab, IDalamudServices dalamudServices)
    {
        this.tab = itemInspectorTab;
        this.dalamudServices = dalamudServices;
        moduleManager.Register(this, false);
        this.dalamudServices.CommandManager.AddHandler("/invorder", new CommandInfo(this.OnCommand));
    }

    private enum Inventory
    {
        Player,
        Retainer,
    }

    /// <inheritdoc />
    public string Name => ModuleName;

    /// <inheritdoc />
    public void Draw() => this.tab.DrawTab();

    /// <inheritdoc />
    public void Enable(bool state = true) => _ = state;

    public void Dispose() => this.dalamudServices.CommandManager.RemoveHandler("/invorder");

    private void OnCommand(string f, string g)
    {
        try
        {
            this.OrderInventory();
        }
        catch (Exception e)
        {
            PluginLog.Error(e, "Error during Test");
            throw;
        }
    }

    private unsafe void OrderInventory()
    {
        IList<(uint, uint)> GetItems(InventoryManager* manager, Inventory inventory)
        {
            var items = new List<(uint, uint)>();
            foreach (var inventoryType in this.InventoryMapping[inventory])
            {
                var container = manager->GetInventoryContainer(inventoryType);
                if (container == null || container->Loaded == 0)
                {
                    continue;
                }

                for (var i = 0; i < container->Size; i++)
                {
                    var item = container->GetInventorySlot(i);
                    if (item->ItemID == 0)
                    {
                        continue;
                    }

                    items.Add((item->ItemID, item->Quantity));
                }
            }

            return items;
        }

        var invManager = InventoryManager.Instance();
        var items = GetItems(invManager, Inventory.Player);
        // PluginLog.Information("{items}", string.Join(items.Select((itemId, count) => $"{itemId}: {count}")));
    }
}