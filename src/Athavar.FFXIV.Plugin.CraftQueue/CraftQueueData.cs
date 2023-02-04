// <copyright file="CraftQueueData.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue;

using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils.Constants;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Lumina.Excel.GeneratedSheets;
using Recipe = Lumina.Excel.GeneratedSheets.Recipe;

internal class CraftQueueData
{
    private readonly List<(Recipe Recipe, Job Job)> recipes = new();
    private readonly List<BuffInfo> foods = new();
    private readonly List<BuffInfo> potions = new();

    public CraftQueueData(IDalamudServices dalamudServices)
    {
        var dataManager = dalamudServices.DataManager;

        // recipes
        this.recipes.AddRange(dataManager.GetExcelSheet<Recipe>()!
           .Where(row => row.RowId != 0U && row.ItemResult.Row > 0U)
           .Select(row => (Recipe: row, Job: row.GetJobType()))
           .Where(row => row.Job > 0)
           .OrderByDescending(row => row.Recipe.RecipeLevelTable.Row)
           .ThenBy(row => row.Job));

        // foods and potions
        var itemFoodSheet = dataManager.GetExcelSheet<ItemFood>()!;
        foreach (var item in dataManager.GetExcelSheet<Item>()!)
        {
            var itemAction = item.ItemAction.Value;
            var dataType = itemAction?.Data[0];
            if (itemAction is null || dataType is not (48 or 49))
            {
                continue;
            }

            var itemFoodRowId = itemAction.Data[1];
            var row = itemFoodSheet.GetRow(itemFoodRowId);
            if (row is null)
            {
                continue;
            }

            StatModifiers nq = new();
            StatModifiers hq = new();
            foreach (var itemFoodUnkData1Obj in row.UnkData1)
            {
                switch ((StatIds)itemFoodUnkData1Obj.BaseParam)
                {
                    case StatIds.CP:
                        nq.CpPct = itemFoodUnkData1Obj.Value;
                        nq.CpMax = itemFoodUnkData1Obj.Max;
                        hq.CpPct = itemFoodUnkData1Obj.ValueHQ;
                        hq.CpMax = itemFoodUnkData1Obj.MaxHQ;
                        break;
                    case StatIds.Craftsmanship:
                        nq.CraftsmanshipPct = itemFoodUnkData1Obj.Value;
                        nq.CraftsmanshipMax = itemFoodUnkData1Obj.Max;
                        hq.CraftsmanshipPct = itemFoodUnkData1Obj.ValueHQ;
                        hq.CraftsmanshipMax = itemFoodUnkData1Obj.MaxHQ;
                        break;
                    case StatIds.Control:
                        nq.ControlPct = itemFoodUnkData1Obj.Value;
                        nq.ControlMax = itemFoodUnkData1Obj.Max;
                        hq.ControlPct = itemFoodUnkData1Obj.ValueHQ;
                        hq.ControlMax = itemFoodUnkData1Obj.MaxHQ;
                        break;
                }
            }

            if (nq.Valid() && hq.Valid())
            {
                if (itemAction.Data[0] == 48)
                {
                    this.foods.Add(new BuffInfo(item, itemFoodRowId, nq, false));
                    this.foods.Add(new BuffInfo(item, itemFoodRowId, hq, true));
                }
                else
                {
                    this.potions.Add(new BuffInfo(item, itemFoodRowId, nq, false));
                    this.potions.Add(new BuffInfo(item, itemFoodRowId, hq, true));
                }
            }
        }

        int SortFunction(BuffInfo a, BuffInfo b)
        {
            var compare = b.Item.LevelItem.Row.CompareTo(a.Item.LevelItem.Row);
            if (compare != 0)
            {
                return compare;
            }

            compare = b.Item.RowId.CompareTo(a.Item.RowId);
            if (compare != 0)
            {
                return compare;
            }

            return b.IsHq == a.IsHq ? 0 : b.IsHq ? 1 : -1;
        }

        this.foods.Sort(SortFunction);
        this.potions.Sort(SortFunction);
    }

    public IReadOnlyList<(Recipe Recipe, Job Job)> Recipes => this.recipes;

    public IReadOnlyList<BuffInfo> Foods => this.foods;

    public IReadOnlyList<BuffInfo> Potions => this.potions;
}