// <copyright file="CraftQueueTab.Queue.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Athavar.FFXIV.Plugin.Extension;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.UI;
using Athavar.FFXIV.Plugin.Utils;
using Athavar.FFXIV.Plugin.Utils.Constants;
using Dalamud;
using Dalamud.Data;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Common.Math;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Action = System.Action;
using Recipe = Lumina.Excel.GeneratedSheets.Recipe;

internal class QueueTab : Tab
{
    private readonly DataManager dataManager;
    private readonly IIconCacheManager iconCacheManager;
    private readonly IGearsetManager gearsetManager;
    private readonly ExcelSheet<Item> itemsSheet;
    private readonly ExcelSheet<ClassJob> classJobsSheet;
    private readonly ExcelSheet<BaseParam> baseParamsSheet;
    private readonly ExcelSheet<Addon> addonsSheet;
    private readonly List<(int Index, Recipe Recipe, Job Job)> filteredRecipes = new();

    private readonly List<(Recipe Recipe, Job Job)> recipes = new();
    private readonly List<BuffInfo> foods = new();
    private readonly List<BuffInfo> potions = new();

    private RotationNode[]? rotations;

    private int recipeIdx = -1;
    private string recipeSearch = string.Empty;

    private int rotationIdx = -1;
    private bool rotationHq;

    private int foodIdx = -1;
    private bool foodHq;

    private int potionIdx = -1;
    private bool potionHq;

    private bool selectionChanged;

    private Lib.CraftSimulation.Models.Recipe? selectedRecipe;
    private RotationNode? selectedRotation;
    private Simulation? craftingSimulation;
    private SimulationResult? simulationResult;

    private bool init;

    public QueueTab(DataManager dataManager, IIconCacheManager iconCacheManager, IGearsetManager gearsetManager, CraftQueueConfiguration configuration, ClientLanguage clientLanguage)
    {
        this.dataManager = dataManager;
        this.iconCacheManager = iconCacheManager;
        this.gearsetManager = gearsetManager;
        this.Configuration = configuration;
        this.ClientLanguage = clientLanguage;

        this.itemsSheet = dataManager.GetExcelSheet<Item>() ?? throw new AthavarPluginException();
        this.classJobsSheet = dataManager.GetExcelSheet<ClassJob>() ?? throw new AthavarPluginException();
        this.baseParamsSheet = dataManager.GetExcelSheet<BaseParam>() ?? throw new AthavarPluginException();
        this.addonsSheet = dataManager.GetExcelSheet<Addon>() ?? throw new AthavarPluginException();
    }

    /// <inheritdoc />
    protected internal override string Name => "Queue";

    /// <inheritdoc />
    protected internal override string Identifier => "Tab-CQQueue";

    private CraftQueueConfiguration Configuration { get; }

    private ClientLanguage ClientLanguage { get; }

    /// <inheritdoc />
    public override void Draw()
    {
        if (!this.init)
        {
            this.init = true;
            this.PopulateData();
        }

        if (this.rotations is null)
        {
            this.rotations = this.Configuration.GetAllNodes().Where(node => node is RotationNode).Cast<RotationNode>().ToArray();
            this.rotationIdx = Array.IndexOf(this.rotations, this.selectedRotation);
        }

        ImGui.Columns(2);

        this.DisplayAddQueueItem();

        ImGui.NextColumn();

        ImGui.Columns(1);
    }

    /// <inheritdoc />
    protected internal override void OnNotDraw()
    {
        if (this.rotations is not null)
        {
            this.rotations = null;
            this.rotationIdx = -1;
        }
    }

    private void PopulateData()
    {
        // recipes
        this.recipes.AddRange(this.dataManager.GetExcelSheet<Recipe>()!
           .Where(row => row.RowId != 0U && row.ItemResult.Row > 0U)
           .Select(row => (Recipe: row, Job: row.GetJobType()))
           .Where(row => row.Job > 0)
           .OrderByDescending(row => row.Recipe.RecipeLevelTable.Row)
           .ThenBy(row => row.Job));
        for (var index = 0; index < this.recipes.Count; ++index)
        {
            this.filteredRecipes.Add((index, this.recipes[index].Recipe, this.recipes[index].Job));
        }

        // foods and potions
        var itemFoodSheet = this.dataManager.GetExcelSheet<ItemFood>()!;
        foreach (var item in this.dataManager.GetExcelSheet<Item>()!)
        {
            var itemAction = item.ItemAction.Value;
            var dataType = itemAction?.Data[0];
            if (itemAction is null || dataType is not 48 or 49)
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
                    this.foods.Add(new BuffInfo(item, itemFoodRowId, nq, hq));
                }
                else
                {
                    this.potions.Add(new BuffInfo(item, itemFoodRowId, nq, hq));
                }
            }
        }

        this.foods.Sort((a, b) => b.Item.LevelItem.Row.CompareTo(a.Item.LevelItem.Row));
        this.potions.Sort((a, b) => b.Item.LevelItem.Row.CompareTo(a.Item.LevelItem.Row));
    }

    private void DisplayAddQueueItem()
    {
        this.DisplayRecipeSelect();
        ImGui.Separator();
        this.DisplayFoodSelect();
        ImGui.Separator();
        this.DisplayPotionSelect();
        ImGui.Separator();
        this.DisplayRotationSelect();
        ImGui.Separator();
        this.RunSimulation();
        this.DisplaySimulationResult();
        this.DisplayCraftingSteps();
    }

    private void DisplayRecipeSelect()
    {
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.BeginCombo("##cq-recipe-list", this.recipeIdx > -1 ? this.recipes[this.recipeIdx].Item1.ItemResult.Value?.Name.RawString ?? "???" : "Select a recipe", ImGuiComboFlags.HeightLargest))
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("##cq-recipe-search", "Search...", ref this.recipeSearch, 512U, ImGuiInputTextFlags.AutoSelectAll))
            {
                this.recipeSearch = this.recipeSearch.Trim();
                this.FilterRecipes(this.recipeSearch);
            }

            if (ImGui.BeginChild("##cq-recipe-list-child", new Vector2(0.0f, 250f) * ImGuiHelpers.GlobalScale, false, ImGuiWindowFlags.NoScrollbar) && ImGui.BeginTable("##cq-recipe-list-table", 4, ImGuiTableFlags.ScrollY))
            {
                ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Job##job", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("rLvl##rlvl", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Name##name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();
                var guiListClipperPtr = ImGuiEx.Clipper(this.filteredRecipes.Count);
                while (guiListClipperPtr.Step())
                {
                    for (var index = guiListClipperPtr.DisplayStart; index < guiListClipperPtr.DisplayEnd; ++index)
                    {
                        ImGui.TableNextRow();
                        var (num, recipe, job) = this.filteredRecipes[index];
                        var obj = recipe.ItemResult.Value;
                        var text = this.classJobsSheet.GetRow((uint)job)?.Abbreviation.RawString ?? "???";
                        ImGui.TableSetColumnIndex(0);
                        Vector2 cursorPos = ImGui.GetCursorPos();
                        if (ImGui.Selectable($"##cq-recipe-{recipe.RowId}", num == this.recipeIdx, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            this.recipeIdx = num;
                            this.selectedRecipe = new Lib.CraftSimulation.Models.Recipe(this.recipes[this.recipeIdx].Recipe, this.itemsSheet);
                            this.SetSimulation();
                            this.selectionChanged = true;
                            ImGui.CloseCurrentPopup();
                        }

                        if (obj is not null && this.iconCacheManager.TryGetIcon(obj.Icon, false, out var textureWrap))
                        {
                            ImGui.SetCursorPos(cursorPos);
                            ImGuiEx.ScaledImageY(textureWrap.ImGuiHandle, textureWrap.Width, textureWrap.Height, ImGui.GetTextLineHeight());
                        }
                        else
                        {
                            ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(text);
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextUnformatted($"{recipe.RecipeLevelTable.Row}");
                        ImGui.TableSetColumnIndex(3);
                        ImGui.TextUnformatted(obj?.Name.RawString);
                    }
                }

                ImGui.EndTable();
                ImGui.EndChild();
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayFoodSelect() => this.DisplayItemPicker("food", this.foods, ref this.foodIdx, ref this.foodHq);

    private void DisplayPotionSelect() => this.DisplayItemPicker("potion", this.potions, ref this.potionIdx, ref this.potionHq);

    private void DisplayItemPicker(
        string id,
        List<BuffInfo> items,
        ref int idx,
        ref bool hq)
    {
        var previewValue = idx == -1 ? "None" : items[idx].Item.Name.RawString;
        if (hq)
        {
            previewValue += " \uE03C";
        }

        if (ImGui.BeginCombo("##" + id, previewValue))
        {
            if (ImGui.BeginTable("##" + id + "-table", 2))
            {
                ImGui.TableSetupColumn("icon", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("name", ImGuiTableColumnFlags.WidthStretch);
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                if (ImGui.Selectable("##none", idx == -1, ImGuiSelectableFlags.SpanAllColumns))
                {
                    idx = -1;
                    hq = false;
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted("None");
                var guiListClipperPtr = ImGuiEx.Clipper(items.Count);

                while (guiListClipperPtr.Step())
                {
                    for (var index = guiListClipperPtr.DisplayStart; index < guiListClipperPtr.DisplayEnd; ++index)
                    {
                        var obj = items[index].Item;
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        Vector2 cursorPos1 = ImGui.GetCursorPos();
                        if (ImGui.Selectable($"##{obj.RowId}-hq", (idx == index) & hq, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            idx = index;
                            hq = true;
                            this.selectionChanged = true;
                        }

                        ImGui.SetCursorPos(cursorPos1);
                        var icon1 = this.iconCacheManager.GetIcon(obj.Icon, true);
                        if (icon1 != null)
                        {
                            ImGuiEx.ScaledImageY(icon1.ImGuiHandle, icon1.Width, icon1.Height, ImGui.GetTextLineHeight());
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(obj.Name.RawString + " \uE03C");
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        Vector2 cursorPos2 = ImGui.GetCursorPos();
                        if (ImGui.Selectable($"##{obj.RowId}-nq", idx == index && !hq, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            idx = index;
                            hq = false;
                            this.selectionChanged = true;
                        }

                        ImGui.SetCursorPos(cursorPos2);
                        var icon2 = this.iconCacheManager.GetIcon(obj.Icon);
                        if (icon2 != null)
                        {
                            ImGuiEx.ScaledImageY(icon2.ImGuiHandle, icon2.Width, icon2.Height, ImGui.GetTextLineHeight());
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(obj.Name.RawString);
                    }
                }

                ImGui.EndTable();
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayRotationSelect()
    {
        var previewValue = this.selectedRotation?.Name ?? "Select a rotation";
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.BeginCombo("##cq-rotation-picker", previewValue))
        {
            if (ImGui.Button("Optimize##cq-rotation-picker-find"))
            {
                this.RotationSolver();
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();
            if (ImGui.Checkbox($"{this.addonsSheet.GetRow(217)!.Text}?##cq-rotation-picker-hq", ref this.rotationHq))
            {
            }

            for (var index = 0; index < this.rotations?.Length; ++index)
            {
                var rotation = this.rotations[index];
                if (ImGui.Selectable($"{rotation.Name}##cq-rotation-{index}", index == this.rotationIdx))
                {
                    this.selectedRotation = rotation;
                    this.rotationIdx = index;
                    this.selectionChanged = true;
                }
            }

            ImGui.EndCombo();
        }
    }

    private void DisplaySimulationResult()
    {
        if (this.craftingSimulation is null || this.selectedRecipe is null)
        {
            return;
        }

        ImGui.TextUnformatted($"{this.classJobsSheet.GetRow(8U + (uint)this.selectedRecipe.Job)!.Name}");

        // stats Craftsmanship and Control
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.CraftsmanshipReq > this.craftingSimulation.CurrentStats.Craftsmanship, ImGuiColors.DalamudRed, $"{this.baseParamsSheet.GetRow(70)!.Name}: {this.craftingSimulation.CurrentStats.Craftsmanship} ");
        ImGui.SameLine();
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.ControlReq > this.craftingSimulation.CurrentStats.Control, ImGuiColors.DalamudRed, $"{this.baseParamsSheet.GetRow(71)!.Name}: {this.craftingSimulation.CurrentStats.Control}");

        ImGuiEx.TextColorCondition(this.simulationResult?.FailCause == SimulationFailCause.NOT_ENOUGH_CP, ImGuiColors.DalamudRed, $"{this.baseParamsSheet.GetRow(11)!.Name}: ");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, ImGuiColors.DalamudViolet);
        ImGui.ProgressBar((float)this.craftingSimulation.AvailableCP / this.craftingSimulation.CurrentStats.CP, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X - 5, ImGui.GetTextLineHeight()), $"{this.craftingSimulation.AvailableCP} / {this.craftingSimulation.CurrentStats.CP}");
        ImGui.PopStyleColor();

        if (this.simulationResult is null)
        {
            return;
        }

        // result
        ImGui.TextUnformatted($"Result {this.simulationResult.Success}. FailCause: {this.simulationResult.FailCause} ");
        ImGui.SameLine();
        ImGuiEx.TextColorCondition(this.simulationResult.FailCause == SimulationFailCause.DURABILITY_REACHED_ZERO, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(214)!.Text}: {this.craftingSimulation.Durability} / {this.craftingSimulation.Recipe.Durability}");

        // Progress line
        var progress = (float)this.craftingSimulation.Progression / this.craftingSimulation.Recipe.Progress;
        var progressText = $"{this.craftingSimulation.Progression}/{this.craftingSimulation.Recipe.Progress}";
        ImGuiEx.TextColorCondition(progress < 1, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(213)!.Text}: {progressText}");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, progress >= 1 ? ImGuiColors.ParsedGreen : ImGuiColors.HealerGreen);
        ImGui.ProgressBar(progress, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X - 5, ImGui.GetTextLineHeight()), progressText);
        ImGui.PopStyleColor();

        // Quality line
        var quality = (float)this.craftingSimulation.Quality / this.craftingSimulation.Recipe.MaxQuality;
        var qualityText = $"{this.craftingSimulation.Quality}/{this.craftingSimulation.Recipe.MaxQuality}";
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.QualityReq > this.craftingSimulation.Quality, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(216)!.Text}: {qualityText}");
        ImGui.SameLine();
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.QualityReq > this.craftingSimulation.Quality, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(217)!.Text}: {this.simulationResult.HqPercent}%");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, quality >= 1 ? ImGuiColors.ParsedBlue : ImGuiColors.TankBlue);
        ImGui.ProgressBar(quality, new System.Numerics.Vector2(ImGui.GetContentRegionAvail().X - 5, ImGui.GetTextLineHeight()), qualityText);
        ImGui.PopStyleColor();

        ImGui.SetNextItemWidth(-1);
        ImGui.Separator();

        var style = ImGui.GetStyle();
        var rotationSteps = this.simulationResult.Steps;
        {
            var size = new System.Numerics.Vector2(22, 22);
            ImGui.GetWindowDrawList().AddRectFilled(new System.Numerics.Vector2(0f, 0f), new System.Numerics.Vector2(100f, 100f), ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudRed));
            for (var index = 0; index < rotationSteps.Count; index++)
            {
                var x = ImGui.GetContentRegionAvail().X;

                var actionResult = rotationSteps[index];
                var tex = this.iconCacheManager.GetIcon(actionResult.Skill.IconId);

                ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 5);
                if (actionResult.Success == false)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, ImGuiColors.DalamudRed);
                }

                ImGui.Image(tex!.ImGuiHandle, new System.Numerics.Vector2(tex.Height, tex.Width));

                if (actionResult.Success == false)
                {
                    ImGui.PopStyleColor();
                }

                ImGui.PopStyleVar();

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(actionResult.Skill.Name[this.ClientLanguage]);
                    ImGui.EndTooltip();
                }

                if (index != rotationSteps.Count - 1 && 80.0 + ImGui.GetStyle().ItemSpacing.X <= x)
                {
                    ImGui.SameLine();
                }
            }
        }


        var valid = this.UpdateQueueChecklist(out var drawChecklistAction);

        if (!valid)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Queue##cq-queue-btn"))
        {
        }

        if (!valid)
        {
            ImGui.EndDisabled();
            if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
            {
                ImGui.BeginTooltip();
                drawChecklistAction?.Invoke();

                ImGui.EndTooltip();
            }
        }
    }

    private void DisplayCraftingSteps()
    {
        if (this.simulationResult is null)
        {
            return;
        }

        var rotationSteps = this.simulationResult.Steps;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.CollapsingHeader("Steps##cq-step-table-collapsing") && ImGui.BeginTable("##cq-step-table", 6, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn("Step##index", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("CP##cp", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Durability##durability", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Quality##quality", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Progress##progress", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();
            var guiListClipperPtr = ImGuiEx.Clipper(rotationSteps.Count);
            while (guiListClipperPtr.Step())
            {
                for (var index = guiListClipperPtr.DisplayStart; index < guiListClipperPtr.DisplayEnd; ++index)
                {
                    var step = rotationSteps[index];
                    if (step.Skipped)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
                    }

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted($"{index}");
                    ImGui.TableSetColumnIndex(1);
                    if (this.iconCacheManager.TryGetIcon(step.Skill.IconId, false, out var textureWrap))
                    {
                        ImGuiEx.ScaledImageY(textureWrap.ImGuiHandle, textureWrap.Width, textureWrap.Height, ImGui.GetTextLineHeight());
                    }
                    else
                    {
                        ImGui.Dummy(new System.Numerics.Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted(step.Skill.Name[this.ClientLanguage]);
                        ImGui.EndTooltip();
                    }

                    ImGui.TableSetColumnIndex(2);
                    ImGuiEx.TextColorCondition(step.FailCause == SimulationFailCause.NOT_ENOUGH_CP, ImGuiColors.DalamudRed, $"{step.CpDifference}");
                    ImGui.TableSetColumnIndex(3);
                    ImGuiEx.TextColorCondition(step.FailCause == SimulationFailCause.DURABILITY_REACHED_ZERO, ImGuiColors.DalamudRed, $"{step.DurabilityDifference}");
                    ImGui.TableSetColumnIndex(4);
                    ImGui.TextUnformatted($"{step.AddedQuality}");
                    ImGui.TableSetColumnIndex(5);
                    ImGui.TextUnformatted($"{step.AddedProgression}");

                    if (step.Skipped)
                    {
                        ImGui.PopStyleColor();
                    }
                }
            }

            ImGui.EndTable();
        }
    }

    private void SetSimulation()
    {
        if (this.selectedRecipe is null)
        {
            return;
        }

        var job = this.selectedRecipe.GameRecipe.GetJobType();

        var gs = this.gearsetManager.AllGearsets.FirstOrDefault(g => g.JobClass == job);
        if (gs == null)
        {
            return;
        }


        this.craftingSimulation = new Simulation(
            new CrafterStats(90, gs.Control, gs.Craftsmanship, gs.CP, gs.HasSoulStone),
            this.selectedRecipe)
        {
            Linear = true,
        };
    }

    private void RunSimulation()
    {
        if (this.selectionChanged && this.craftingSimulation is not null && this.selectedRotation is not null)
        {
            this.simulationResult = this.craftingSimulation.Run(this.selectedRotation.Rotations, this.GetCurrentStatBuffs().ToArray());
            this.selectionChanged = false;
        }
    }

    private void FilterRecipes(string needle)
    {
        if (string.IsNullOrEmpty(needle))
        {
            this.filteredRecipes.Clear();
            for (var index = 0; index < this.recipes.Count; ++index)
            {
                this.filteredRecipes.Add((index, this.recipes[index].Recipe, this.recipes[index].Job));
            }
        }
        else
        {
            needle = needle.ToLowerInvariant();
            this.filteredRecipes.Clear();
            for (var index = 0; index < this.recipes.Count; ++index)
            {
                var (recipe, job) = this.recipes[index];
                if (recipe.ItemResult.Value != null && recipe.ItemResult.Value.Name.RawString.ToLowerInvariant().Contains(needle))
                {
                    this.filteredRecipes.Add((index, recipe, job));
                }
            }
        }
    }

    private bool UpdateQueueChecklist([NotNullWhen(false)] out Action? action)
    {
        var recipeSelected = this.recipeIdx > -1;
        var rotationSelected = this.selectedRotation is not null;
        var haveGearset = this.simulationResult?.Success == true;
        var valid = recipeSelected & rotationSelected;

        if (!valid)
        {
            action = () =>
            {
                ImGui.PushTextWrapPos();
                ImGui.BeginDisabled();
                ImGui.Checkbox("Recipe selected", ref recipeSelected);
                /*ImGui.Checkbox("Have ingredients (considering queue)", ref haveIngredients);
                ImGui.Checkbox("All ingredients selected", ref allIngredientsSelected);*/
                ImGui.Spacing();
                ImGui.Checkbox("Rotation selected", ref rotationSelected);
                ImGui.Checkbox("Have gearset with required stats", ref haveGearset);
                /*ImGui.Checkbox("Have food", ref haveFood);
                ImGui.Checkbox("Have potion", ref havePotion);*/
                ImGui.EndDisabled();
                ImGui.PopTextWrapPos();
            };
        }
        else
        {
            action = null;
        }

        return valid;
    }

    private IEnumerable<StatModifiers> GetCurrentStatBuffs()
    {
        if (this.foodIdx > 0)
        {
            var food = this.foods[this.foodIdx];
            if (this.foodHq)
            {
                yield return food.Hq;
            }
            else
            {
                yield return food.Nq;
            }
        }

        if (this.potionIdx > 0)
        {
            var potion = this.potions[this.potionIdx];
            if (this.potionHq)
            {
                yield return potion.Hq;
            }
            else
            {
                yield return potion.Nq;
            }
        }
    }

    private void RotationSolver()
    {
        if (this.rotations is null || this.craftingSimulation is null)
        {
            return;
        }

        var currentStatModifier = this.GetCurrentStatBuffs().ToArray();

        List<(RotationNode Node, SimulationResult Result)> success = new();
        foreach (var rotationNode in this.rotations)
        {
            var result = this.craftingSimulation.Run(rotationNode.Rotations, currentStatModifier);
            if (result.Success)
            {
                success.Add((rotationNode, result));
            }
        }

        if (success.Count == 0)
        {
            return;
        }

        var winner = (this.rotationHq ? success.OrderByDescending(s => s.Result.HqPercent) : success.OrderBy(s => s.Result.Steps.Count(ar => !ar.Skipped)))
           .FirstOrDefault();

        this.selectedRotation = winner.Node;
        this.rotationIdx = Array.IndexOf(this.rotations, this.selectedRotation);
        this.selectionChanged = true;
    }
}