// <copyright file="Condition.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro
{
    using System.Linq;

    using Lumina.Excel;

    /// <summary>
    /// Contains condition checks.
    /// </summary>
    internal static class Condition
    {
        private static ExcelSheet<Lumina.Excel.GeneratedSheets.Status>? statusSheet;

        /// <summary>
        /// Defines the different categories of conditions.
        /// </summary>
        internal enum Category
        {
            /// <summary>
            /// Checks for existing status on player.
            /// </summary>
            PlayerStatus,

            /// <summary>
            /// Checks for player equipment.
            /// </summary>
            Equipment,
        }

        /// <summary>
        /// Checks if a condition is fulfilled.
        /// </summary>
        /// <param name="category">The category of the condition.</param>
        /// <param name="condition">The condition parameter.</param>
        /// <returns>Indicates if the condition is fulfilled or not.</returns>
        public static bool ConditionCheck(Category category, string condition)
        {
            switch (category)
            {
                case Category.PlayerStatus:
                    return CheckPlayerStatus(condition.ToLowerInvariant());
                case Category.Equipment:
                    return CheckEquipment(condition.ToLowerInvariant());
            }

            return false;
        }

        /// <summary>
        /// Checks if status is existing in current player status list.
        /// </summary>
        /// <param name="statusName">Name of status effect.</param>
        /// <returns>Indicates if is present or not.</returns>
        public static bool CheckPlayerStatus(string statusName)
        {
            statusSheet ??= DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>()!;

            var effectIDs = statusSheet.Where(row => row.Name.RawString.ToLower() == statusName).Select(row => row.RowId).ToList();

            return DalamudBinding.ClientState.LocalPlayer?.StatusList.Select(se => se.StatusId).ToList().Intersect(effectIDs).Any() ?? false;
        }

        /// <summary>
        /// Checks if equipment is fulfilling a condition.
        /// </summary>
        /// <param name="condition">Name of status effect.</param>
        /// <returns>Indicates if is present or not.</returns>
        public static bool CheckEquipment(string condition)
        {
            switch (condition.ToLowerInvariant())
            {
                case "condition":
                    return Modules.Instance.EquipmentScanner.GetLowestCondition() > 0;
            }

            return false;
        }
    }
}
