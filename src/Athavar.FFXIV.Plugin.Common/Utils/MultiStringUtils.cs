// <copyright file="MultiStringUtils.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Utils;

using Athavar.FFXIV.Plugin.Models;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

public static class MultiStringUtils
{
    public static MultiString FromPlaceName(IDataManager gameData, uint id) => MultiString.From<PlaceName>(gameData, id, item => item?.Name);

    public static MultiString FromItem(IDataManager gameData, uint id) => MultiString.From<Item>(gameData, id, item => item?.Name);

    public static MultiString FromAction(IDataManager gameData, uint id) => MultiString.From<Action>(gameData, id, item => item?.Name);

    public static MultiString FromCraftAction(IDataManager gameData, uint id) => MultiString.From<CraftAction>(gameData, id, item => item?.Name);

    public static MultiString FromContentFinderCondition(IDataManager gameData, uint id) => MultiString.From<ContentFinderCondition>(gameData, id, item => item?.Name);
}