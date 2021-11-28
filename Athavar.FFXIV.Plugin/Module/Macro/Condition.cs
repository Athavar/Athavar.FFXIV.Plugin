// <copyright file="Condition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

/// <summary>
///     Contains condition checks.
/// </summary>
internal class Condition
{
    private readonly IDalamudServices dalamudServices;
    private readonly EquipmentScanner equipmentScanner;
    private ExcelSheet<Status>? statusSheet;

    public Condition(IDalamudServices dalamudServices, EquipmentScanner equipmentScanner)
    {
        this.dalamudServices = dalamudServices;
        this.equipmentScanner = equipmentScanner;
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
    /// <returns>Indicates if the condition is fulfilled or not.</returns>
    public bool ConditionCheck(Category category, string condition)
    {
        switch (category)
        {
            case Category.PlayerStatus:
                return this.CheckPlayerStatus(condition.ToLowerInvariant());
            case Category.Equipment:
                return this.CheckEquipment(condition.ToLowerInvariant());
        }

        return false;
    }

    /// <summary>
    ///     Checks if status is existing in current player status list.
    /// </summary>
    /// <param name="statusName">Name of status effect.</param>
    /// <returns>Indicates if is present or not.</returns>
    public bool CheckPlayerStatus(string statusName)
    {
        this.statusSheet ??= this.dalamudServices.DataManager.GetExcelSheet<Status>()!;

        var effectIDs = this.statusSheet.Where(row => row.Name.RawString.ToLower() == statusName).Select(row => row.RowId).ToList();

        return this.dalamudServices.ClientState.LocalPlayer?.StatusList.Select(se => se.StatusId).ToList().Intersect(effectIDs).Any() ?? false;
    }

    /// <summary>
    ///     Checks if equipment is fulfilling a condition.
    /// </summary>
    /// <param name="condition">Name of status effect.</param>
    /// <returns>Indicates if is present or not.</returns>
    public bool CheckEquipment(string condition)
    {
        switch (condition.ToLowerInvariant())
        {
            case "condition":
                return this.equipmentScanner.GetLowestCondition() > 0;
        }

        return false;
    }
}