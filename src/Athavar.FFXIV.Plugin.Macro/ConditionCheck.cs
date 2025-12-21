// <copyright file="ConditionCheck.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Lumina.Excel;
using Lumina.Excel.Sheets;

/// <summary>
///     Contains condition checks.
/// </summary>
internal sealed class ConditionCheck
{
    private readonly IDalamudServices dalamudServices;
    private readonly ICommandInterface commandInterface;
    private ExcelSheet<Status>? statusSheet;

    public ConditionCheck(IDalamudServices dalamudServices, ICommandInterface commandInterface)
    {
        this.dalamudServices = dalamudServices;
        this.commandInterface = commandInterface;
    }

    /// <summary>
    ///     Defines the different categories of conditions.
    /// </summary>
    internal enum Category
    {
        /// <summary>
        ///     Checks for existing status on player.
        /// </summary>
        PlayerStatus,

        /// <summary>
        ///     Checks for player equipment.
        /// </summary>
        Equipment,
    }

    /// <summary>
    ///     Checks if a condition is fulfilled.
    /// </summary>
    /// <param name="category">The category of the condition.</param>
    /// <param name="condition">The condition parameter.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Indicates if the condition is fulfilled or not.</returns>
    public Task<bool> Check(Category category, string condition, CancellationToken token)
    {
        switch (category)
        {
            case Category.PlayerStatus:
                if (this.dalamudServices.Framework.IsInFrameworkUpdateThread)
                {
                    return Task.FromResult(this.CheckPlayerStatus(condition));
                }

                // TODO: fix running, currently freeze/hangs game
                return Task.FromResult(false);
                return this.dalamudServices.Framework.RunOnTick(() => this.CheckPlayerStatus(condition.ToLowerInvariant()), cancellationToken: token);
            case Category.Equipment:
                return this.CheckEquipment(condition.ToLowerInvariant());
        }

        return Task.FromResult(false);
    }

    /// <summary>
    ///     Checks if status is existing in current player status list.
    /// </summary>
    /// <param name="statusName">Name of status effect.</param>
    /// <returns>Indicates if is present or not.</returns>
    public bool CheckPlayerStatus(string statusName)
    {
        this.statusSheet ??= this.dalamudServices.DataManager.GetExcelSheet<Status>()!;

        var effectIDs = this.statusSheet.Where(row => row.Name.ExtractText().ToLower() == statusName).Select(row => row.RowId).ToList();

        return this.dalamudServices.ObjectTable.LocalPlayer?.StatusList
           .Select(se => se.StatusId)
           .ToList()
           .Intersect(effectIDs)
           .Any() ?? false;
    }

    /// <summary>
    ///     Checks if equipment is fulfilling a condition.
    /// </summary>
    /// <param name="condition">Name of status effect.</param>
    /// <returns>Indicates if is present or not.</returns>
    public Task<bool> CheckEquipment(string condition)
    {
        switch (condition.ToLowerInvariant())
        {
            case "condition":
                return Task.FromResult(this.commandInterface.NeedsRepair());
        }

        return Task.FromResult(false);
    }
}