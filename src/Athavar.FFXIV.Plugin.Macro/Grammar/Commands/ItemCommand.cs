// <copyright file="ItemCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Sheets = Lumina.Excel.Sheets;

/// <summary>
///     The /item command.
/// </summary>
[MacroCommand("item", null, "Use an item, stopping the macro if the item is not present.", ["hq", "wait"], ["/item Calamari Ripieni", "/item Calamari Ripieni <hq> <wait.3>"], RequireLogin = true)]
internal class ItemCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/item\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string itemName;
    private readonly ItemQualityModifier itemQualityMod;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ItemCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="itemName">Item name.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="itemQualityMod">Required quality of the item used.</param>
    private ItemCommand(string text, string itemName, WaitModifier wait, ItemQualityModifier itemQualityMod)
        : base(text, wait)
    {
        this.itemName = itemName.ToLowerInvariant();
        this.itemQualityMod = itemQualityMod;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static ItemCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = ItemQualityModifier.TryParse(ref text, out var itemQualityModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = match.ExtractAndUnquote("name");

        return new ItemCommand(text, nameValue, waitModifier, itemQualityModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        this.Logger.Debug($"Executing: {this.Text}");

        var itemId = this.SearchItemId(this.itemName);
        this.Logger.Debug($"Item found: {itemId}");

        var count = this.GetInventoryItemCount(itemId, this.itemQualityMod.IsHq);
        this.Logger.Debug($"Item Count: {count}");
        if (count == 0)
        {
            throw new MacroCommandError("You do not have that item");
        }

        this.UseItem(itemId, this.itemQualityMod.IsHq);

        await this.PerformWait(token);
    }

    private unsafe void UseItem(uint itemId, bool isHQ = false)
    {
        var agent = AgentInventoryContext.Instance();
        if (agent == null)
        {
            throw new MacroCommandError("AgentInventoryContext not found");
        }

        if (isHQ)
        {
            itemId += 1_000_000;
        }

        var result = agent->UseItem(itemId);
        if (result != 0)
        {
            throw new MacroCommandError("Failed to use item");
        }
    }

    private unsafe int GetInventoryItemCount(uint itemId, bool isHQ)
    {
        var inventoryManager = InventoryManager.Instance();
        if (inventoryManager == null)
        {
            throw new MacroCommandError("InventoryManager not found");
        }

        return inventoryManager->GetInventoryItemCount(itemId, isHQ);
    }

    private uint SearchItemId(string itemName)
    {
        var sheet = DalamudServices.DataManager.GetExcelSheet<Sheets.Item>()!;
        Sheets.Item? item = sheet.FirstOrDefault(r => r.Name.ToString().ToLowerInvariant() == itemName);
        if (item == null)
        {
            throw new MacroCommandError("Item not found");
        }

        return item?.RowId ?? 0;
    }
}