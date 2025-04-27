// <copyright file="WKSMissionUnitExtensions.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.Extension;

using Athavar.FFXIV.Plugin.CraftQueue.UI;
using Athavar.FFXIV.Plugin.Models;
using Lumina.Excel.Sheets;

internal static class WKSExtensions
{
    public static Job GetJob(this WKSMissionUnit missionUnit) => (Job)missionUnit.Unknown1;

    public static IEnumerable<ushort> GetMissionTodoRowId(this WKSMissionUnit missionUnit)
    {
        if (missionUnit.Unknown7 != 0)
        {
            yield return missionUnit.Unknown7;
        }

        if (missionUnit.Unknown8 != 0)
        {
            yield return missionUnit.Unknown8;
        }

        if (missionUnit.Unknown9 != 0)
        {
            yield return missionUnit.Unknown9;
        }
    }

    public static IEnumerable<(ushort ItemInfoRow, ushort Quantity)> GetRequiredItem(this CosmicTab.WKSMissionToDo2 missionToDo)
    {
        if (missionToDo.Unknown3 != 0)
        {
            yield return (missionToDo.Unknown3, missionToDo.Unknown6);
        }

        if (missionToDo.Unknown4 != 0)
        {
            yield return (missionToDo.Unknown4, missionToDo.Unknown7);
        }

        if (missionToDo.Unknown5 != 0)
        {
            yield return (missionToDo.Unknown5, missionToDo.Unknown8);
        }
    }
}