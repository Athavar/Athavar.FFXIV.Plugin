// <copyright file="QueueTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftQueue.Extension;
using Athavar.FFXIV.Plugin.CraftSimulator;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Action = System.Action;
using CurrentIngredient = Tuple<(uint ItemId, ushort Icon, byte Amount, ushort NqCount, uint NqAvailable, bool HaveAllNq, int HqIndex, ushort HqCount, uint HqAvailable, bool HaveAllHq)>;
using Recipe = Lumina.Excel.GeneratedSheets.Recipe;

internal sealed class QueueTab : Tab
{
    private readonly CraftQueue craftQueue;
    private readonly CraftQueueData craftQueueData;
    private readonly IIconManager iconManager;
    private readonly ICraftDataManager craftDataManager;
    private readonly IGearsetManager gearsetManager;
    private readonly ICommandInterface ci;
    private readonly ExcelSheet<Item> itemsSheet;
    private readonly ExcelSheet<ClassJob> classJobsSheet;
    private readonly ExcelSheet<BaseParam> baseParamsSheet;
    private readonly ExcelSheet<Addon> addonsSheet;
    private readonly string potionLabel;
    private readonly string foodLabel;
    private readonly List<(int Index, Recipe Recipe, Job Job)> filteredRecipes = new();

    private int craftCount = 1;

    private RotationNode[]? rotations;

    private int recipeIdx = -1;
    private string recipeSearch = string.Empty;

    private int rotationIdx = -1;
    private bool rotationHq;

    private int foodIdx = -1;
    private int potionIdx = -1;

    private bool selectionChanged;

    private CraftSimulator.Models.Recipe? selectedRecipe;
    private RotationNode? selectedRotation;
    private BuffInfo? selectedFood;
    private BuffInfo? selectedPotion;
    private (uint ItemId, byte Amount)[] hqIngredients = Array.Empty<(uint ItemId, byte Amount)>();
    private CurrentIngredient[] currentIngredients = Array.Empty<CurrentIngredient>();
    private bool haveAllIngredient;

    private Simulation? craftingSimulation;
    private SimulationResult? simulationResult;

    private bool init;
    private uint updateTick;

    public QueueTab(IIconManager iconManager, ICraftDataManager craftDataManager, CraftQueue craftQueue, CraftQueueConfiguration configuration, ClientLanguage clientLanguage)
    {
        this.craftQueue = craftQueue;
        this.craftQueueData = craftQueue.Data;
        this.ci = craftQueue.CommandInterface;
        this.gearsetManager = craftQueue.GearsetManager;
        this.craftDataManager = craftDataManager;
        this.iconManager = iconManager;
        this.Configuration = configuration;
        this.ClientLanguage = clientLanguage;
        var dataManager = craftQueue.DalamudServices.DataManager;

        this.itemsSheet = dataManager.GetExcelSheet<Item>() ?? throw new AthavarPluginException();
        this.classJobsSheet = dataManager.GetExcelSheet<ClassJob>() ?? throw new AthavarPluginException();
        this.baseParamsSheet = dataManager.GetExcelSheet<BaseParam>() ?? throw new AthavarPluginException();
        this.addonsSheet = dataManager.GetExcelSheet<Addon>() ?? throw new AthavarPluginException();
        var sheet = dataManager.GetExcelSheet<ItemSearchCategory>();
        this.foodLabel = sheet?.GetRow(45)?.Name ?? string.Empty;
        this.potionLabel = sheet?.GetRow(43)?.Name ?? string.Empty;
    }

    /// <inheritdoc/>
    public override string Name => "Queue";

    /// <inheritdoc/>
    public override string Identifier => "Tab-CQQueue";

    private CraftQueueConfiguration Configuration { get; }

    private ClientLanguage ClientLanguage { get; }

    /// <inheritdoc/>
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

        if (!this.craftQueue.DalamudServices.ClientState.IsLoggedIn)
        {
            ImGui.TextUnformatted("Please login.");
            return;
        }

        this.updateTick++;
        if (this.updateTick > 10)
        {
            this.UpdateCurrentIngredients();
            this.updateTick = 0;
        }

        ImGui.Columns(2);

        this.DisplayAddQueueItem();

        ImGui.NextColumn();

        this.DisplayQueueAndHistory();

        ImGui.Columns(1);
    }

    /// <inheritdoc/>
    public override void OnNotDraw()
    {
        if (this.rotations is not null)
        {
            this.rotations = null;
            this.rotationIdx = -1;
        }
    }

    private void PopulateData()
    {
        for (var index = 0; index < this.craftQueueData.Recipes.Count; ++index)
        {
            var recipe = this.craftQueueData.Recipes[index];
            this.filteredRecipes.Add((index, recipe.Recipe, recipe.Job));
        }
    }

    private void DisplayAddQueueItem()
    {
        if (ImGui.BeginChild("##craftqueue-addqueue"))
        {
            this.DisplayRecipeSelect();
            ImGui.Separator();
            this.DisplayFoodSelect();
            ImGui.Separator();
            this.DisplayPotionSelect();
            ImGui.Separator();
            this.DisplayRotationSelect();
            ImGui.Separator();
            this.DisplayJobEnqueue();
            ImGui.Separator();
            this.DisplayIngredient();
            ImGui.Separator();
            this.RunSimulation();
            this.DisplaySimulationResult();
            this.DisplayCraftingSteps();
            ImGui.EndChild();
        }
    }

    private void DisplayRecipeSelect()
    {
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.BeginCombo("##recipe-list", this.recipeIdx > -1 ? this.craftQueueData.Recipes[this.recipeIdx].Recipe.ItemResult.Value?.Name.ToDalamudString().TextValue ?? "???" : "Select a recipe", ImGuiComboFlags.HeightLargest))
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("##recipe-search", "Search...", ref this.recipeSearch, 512U, ImGuiInputTextFlags.AutoSelectAll))
            {
                this.recipeSearch = this.recipeSearch.Trim();
                this.FilterRecipes(this.recipeSearch);
            }

            if (!ImGui.IsAnyItemFocused() && !ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                ImGui.SetKeyboardFocusHere(-1);
            }

            if (ImGui.BeginChild("##recipe-list-child", new Vector2(0.0f, 250f) * ImGuiHelpers.GlobalScale, false, ImGuiWindowFlags.NoScrollbar) && ImGui.BeginTable("##cq-recipe-list-table", 4, ImGuiTableFlags.ScrollY))
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
                        var cursorPos = ImGui.GetCursorPos();
                        if (ImGui.Selectable($"##recipe-{recipe.RowId}", num == this.recipeIdx, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            this.recipeIdx = num;
                            this.selectedRecipe = this.craftQueueData.Recipes[this.recipeIdx].Recipe.ToCraftSimulatorRecipe(this.itemsSheet);
                            this.hqIngredients = this.selectedRecipe.Ingredients.Where(i => i.CanBeHq).Select(i => (ItemId: i.Id, Amount: (byte)0)).ToArray();
                            this.craftCount = 1;
                            this.UpdateCurrentIngredients();
                            this.SetSimulation();
                            this.selectionChanged = true;
                            ImGui.CloseCurrentPopup();
                        }

                        if (obj is not null && this.iconManager.TryGetIcon(obj.Icon, out var textureWrap))
                        {
                            ImGui.SetCursorPos(cursorPos);
                            ImGuiEx.ScaledImageY(textureWrap.ImGuiHandle, textureWrap.Width, textureWrap.Height, ImGui.GetTextLineHeight());
                        }
                        else
                        {
                            ImGui.Dummy(new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(text);
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextUnformatted($"{recipe.RecipeLevelTable.Row}");
                        ImGui.TableSetColumnIndex(3);
                        ImGui.TextUnformatted(obj?.Name.ToDalamudString().TextValue);
                    }
                }

                guiListClipperPtr.End();
                guiListClipperPtr.Destroy();

                ImGui.EndTable();
                ImGui.EndChild();
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayFoodSelect() => this.DisplayItemPicker(this.foodLabel, "food", this.craftQueueData.Foods, ref this.foodIdx, ref this.selectedFood);

    private void DisplayPotionSelect() => this.DisplayItemPicker(this.potionLabel, "potion", this.craftQueueData.Potions, ref this.potionIdx, ref this.selectedPotion);

    private void DisplayItemPicker(
        string label,
        string id,
        IReadOnlyList<BuffInfo> items,
        ref int idx,
        ref BuffInfo? selected)
    {
        var previewValue = selected == null ? "None" : selected.Name;
        if (selected?.IsHq == true)
        {
            previewValue += " \uE03C";
        }

        if (ImGui.BeginCombo(label + "##" + id, previewValue))
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
                    selected = null;
                    this.selectionChanged = true;
                }

                ImGui.TableSetColumnIndex(1);
                ImGui.TextUnformatted("None");
                var guiListClipperPtr = ImGuiEx.Clipper(items.Count);

                while (guiListClipperPtr.Step())
                {
                    for (var index = guiListClipperPtr.DisplayStart; index < guiListClipperPtr.DisplayEnd; ++index)
                    {
                        var buffInfo = items[index];
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        var cursorPos1 = ImGui.GetCursorPos();
                        if (ImGui.Selectable($"##{index}", idx == index, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            idx = index;
                            selected = items[index];
                            this.selectionChanged = true;
                        }

                        ImGui.SetCursorPos(cursorPos1);
                        var icon = this.iconManager.GetIcon(buffInfo.IconId, ITextureProvider.IconFlags.HiRes | (buffInfo.IsHq ? ITextureProvider.IconFlags.ItemHighQuality : 0));
                        if (icon != null)
                        {
                            ImGuiEx.ScaledImageY(icon.ImGuiHandle, icon.Width, icon.Height, ImGui.GetTextLineHeight());
                        }

                        ImGui.TableSetColumnIndex(1);
                        if (buffInfo.IsHq)
                        {
                            ImGui.TextUnformatted(buffInfo.Name + " \uE03C");
                        }
                        else
                        {
                            ImGui.TextUnformatted(buffInfo.Name);
                        }
                    }
                }

                guiListClipperPtr.End();
                guiListClipperPtr.Destroy();

                ImGui.EndTable();
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayRotationSelect()
    {
        var previewValue = this.selectedRotation?.Name ?? "Select a rotation";
        if (ImGui.BeginCombo("##cq-rotation-picker", previewValue))
        {
            ImGuiEx.TextTooltip("Try to find rotation with the best HQ percentage");

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

        ImGui.SameLine();
        if (ImGui.Button("Optimize##cq-rotation-picker-find"))
        {
            this.RotationSolver();
        }

        ImGui.SameLine();
        if (ImGui.Checkbox($"{this.addonsSheet.GetRow(217)!.Text}?##cq-rotation-picker-hq", ref this.rotationHq))
        {
        }
    }

    private void DisplayJobEnqueue()
    {
        if (ImGui.InputInt("Quantity", ref this.craftCount))
        {
            if (this.craftCount <= 0)
            {
                this.craftCount = 1;
            }

            this.UpdateCurrentIngredients();
        }

        ImGui.SameLine();

        var valid = this.UpdateQueueChecklist(out var drawChecklistAction);

        if (!valid)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Queue##queue-btn") && this.selectedRecipe is not null && this.selectedRotation is not null)
        {
            this.craftQueue.CreateJob(this.selectedRecipe, this.selectedRotation, (uint)this.craftCount, this.selectedFood, this.selectedPotion, this.hqIngredients);
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

    private void DisplayIngredient()
    {
        if (this.selectedRecipe is null)
        {
            return;
        }

        if (ImGui.BeginTable("##ingredient-table", 4))
        {
            ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Amt", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("NQ", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("HQ", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableHeadersRow();
            for (var index = 0; index < this.currentIngredients.Length; ++index)
            {
                var ingredient = this.currentIngredients[index].Item1;
                if (ingredient.ItemId != 0)
                {
                    ImGui.TableNextRow();
                    if (ImGui.TableSetColumnIndex(0) && this.iconManager.TryGetIcon(ingredient.Icon, out var textureWrap))
                    {
                        ImGuiEx.ScaledImageY(textureWrap.ImGuiHandle, textureWrap.Width, textureWrap.Height, ImGui.GetTextLineHeight());
                        ImGuiEx.TextTooltip(this.itemsSheet.GetRow(ingredient.ItemId)?.Name.ToDalamudString().TextValue ?? string.Empty);
                    }

                    if (ImGui.TableSetColumnIndex(1))
                    {
                        ImGui.TextUnformatted(ingredient.Amount.ToString());
                    }

                    if (ImGui.TableSetColumnIndex(2))
                    {
                        if (ingredient.HqIndex != -1)
                        {
                            ImGuiEx.TextColorCondition(!ingredient.HaveAllNq, ImGuiColors.DalamudRed, $"({ingredient.NqCount * this.craftCount})");
                            ImGui.SameLine();
                            int quantity = ingredient.NqCount;
                            if (DrawItemPicker($"/ {ingredient.NqAvailable}##nq-ing-{index}", ingredient.Amount, ref quantity))
                            {
                                this.hqIngredients[ingredient.HqIndex] = (ingredient.ItemId, (byte)(ingredient.Amount - quantity));
                                this.craftingSimulation?.SetHqIngredients(this.hqIngredients);
                                this.selectionChanged = true;
                                this.UpdateCurrentIngredients();
                            }
                        }
                        else
                        {
                            ImGuiEx.TextColorCondition(!ingredient.HaveAllNq, ImGuiColors.DalamudRed, $"{ingredient.NqCount * this.craftCount}");
                            ImGui.SameLine();
                            ImGui.TextUnformatted($"/ {ingredient.NqAvailable}");
                        }
                    }

                    if (ImGui.TableSetColumnIndex(3) && ingredient.HqIndex != -1)
                    {
                        ImGuiEx.TextColorCondition(!ingredient.HaveAllHq, ImGuiColors.DalamudRed, $"({ingredient.HqCount * this.craftCount})");
                        ImGui.SameLine();
                        int quantity = ingredient.HqCount;
                        if (DrawItemPicker($"/ {ingredient.HqAvailable}##hq-ing-{index}", ingredient.Amount, ref quantity))
                        {
                            this.hqIngredients[ingredient.HqIndex] = (ingredient.ItemId, (byte)quantity);
                            this.craftingSimulation?.SetHqIngredients(this.hqIngredients);
                            this.selectionChanged = true;
                            this.UpdateCurrentIngredients();
                        }
                    }
                }

                bool DrawItemPicker(string id, int total, ref int quality)
                {
                    if (!ImGui.InputInt(id, ref quality))
                    {
                        return false;
                    }

                    quality = Math.Min(Math.Max(0, quality), total);
                    return true;
                }
            }

            ImGui.EndTable();
        }
    }

    private void DisplaySimulationResult()
    {
        if (this.craftingSimulation is null || this.selectedRecipe is null)
        {
            return;
        }

        ImGui.TextUnformatted($"{this.classJobsSheet.GetRow(this.selectedRecipe.Class.GetRowId())!.Name}");

        // stats Craftsmanship and Control
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.CraftsmanshipReq > this.craftingSimulation.CurrentStats.Craftsmanship, ImGuiColors.DalamudRed, $"{this.baseParamsSheet.GetRow(70)!.Name}: {this.craftingSimulation.CurrentStats.Craftsmanship} ");
        ImGui.SameLine();
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.ControlReq > this.craftingSimulation.CurrentStats.Control, ImGuiColors.DalamudRed, $"{this.baseParamsSheet.GetRow(71)!.Name}: {this.craftingSimulation.CurrentStats.Control}");

        ImGuiEx.TextColorCondition(this.simulationResult?.FailCause == SimulationFailCause.NOT_ENOUGH_CP, ImGuiColors.DalamudRed, $"{this.baseParamsSheet.GetRow(11)!.Name}: ");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, ImGuiColors.DalamudViolet);
        ImGui.ProgressBar((float)this.craftingSimulation.AvailableCP / this.craftingSimulation.CurrentStats.CP, new Vector2(ImGui.GetContentRegionAvail().X - 5, ImGui.GetTextLineHeight()), $"{this.craftingSimulation.AvailableCP} / {this.craftingSimulation.CurrentStats.CP}");
        ImGui.PopStyleColor();

        ImGui.Separator();

        if (this.simulationResult is null || this.selectedRotation is null)
        {
            return;
        }

        // result
        ImGuiEx.TextColorCondition(this.simulationResult.FailCause == SimulationFailCause.DURABILITY_REACHED_ZERO, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(214)!.Text}: {this.craftingSimulation.Durability} / {this.craftingSimulation.Recipe.Durability}");

        // Progress line
        var progress = (float)this.craftingSimulation.Progression / this.craftingSimulation.Recipe.Progress;
        var progressText = $"{this.craftingSimulation.Progression}/{this.craftingSimulation.Recipe.Progress}";
        ImGuiEx.TextColorCondition(progress < 1, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(213)!.Text}: {progressText}");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, progress >= 1 ? ImGuiColors.ParsedGreen : ImGuiColors.HealerGreen);
        ImGui.ProgressBar(progress, new Vector2(ImGui.GetContentRegionAvail().X - 5, ImGui.GetTextLineHeight()), progressText);
        ImGui.PopStyleColor();

        // Quality line
        var quality = (float)this.craftingSimulation.Quality / this.craftingSimulation.Recipe.MaxQuality;
        var qualityText = $"{this.craftingSimulation.Quality}/{this.craftingSimulation.Recipe.MaxQuality}";
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.QualityReq > this.craftingSimulation.Quality, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(216)!.Text}: {qualityText}");
        ImGui.SameLine();
        ImGuiEx.TextColorCondition(this.craftingSimulation.Recipe.QualityReq > this.craftingSimulation.Quality, ImGuiColors.DalamudRed, $"{this.addonsSheet.GetRow(217)!.Text}: {this.simulationResult.HqPercent}%");
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.PlotHistogram, quality >= 1 ? ImGuiColors.ParsedBlue : ImGuiColors.TankBlue);
        ImGui.ProgressBar(quality, new Vector2(ImGui.GetContentRegionAvail().X - 5, ImGui.GetTextLineHeight()), qualityText);
        ImGui.PopStyleColor();

        ImGui.SetNextItemWidth(-1);
        ImGui.Separator();

        var rotationSteps = this.simulationResult.Steps;
        {
            var classIndex = (int)this.selectedRecipe.Class;
            for (var index = 0; index < rotationSteps.Count; index++)
            {
                var x = ImGui.GetContentRegionAvail().X;

                var actionResult = rotationSteps[index];
                var craftSkillData = this.craftDataManager.GetCraftSkillData(actionResult.Skill);
                var tex = this.iconManager.GetIcon(craftSkillData.IconIds[classIndex]);
                if (tex is null)
                {
                    continue;
                }

                var iconSize = new Vector2(tex.Height, tex.Width);

                ImGui.Image(tex.ImGuiHandle, iconSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(craftSkillData.Name[this.ClientLanguage] + (actionResult.FailCause is not null ? ": " + actionResult.FailCause : string.Empty));
                    ImGui.EndTooltip();
                }

                if (index != rotationSteps.Count - 1 && 80.0 + ImGui.GetStyle().ItemSpacing.X <= x)
                {
                    ImGui.SameLine();
                }
            }
        }
    }

    private void DisplayCraftingSteps()
    {
        if (this.simulationResult is null || this.selectedRecipe is null)
        {
            return;
        }

        var rotationSteps = this.simulationResult.Steps;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.CollapsingHeader("Steps##step-table-collapsing") && ImGui.BeginTable("##step-table", 6, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders))
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
            var classIndex = (int)this.selectedRecipe.Class;
            while (guiListClipperPtr.Step())
            {
                for (var index = guiListClipperPtr.DisplayStart; index < guiListClipperPtr.DisplayEnd; ++index)
                {
                    var step = rotationSteps[index];
                    var craftSkillData = this.craftDataManager.GetCraftSkillData(step.Skill);
                    if (step.Skipped)
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
                    }

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    ImGui.TextUnformatted($"{index}");
                    ImGui.TableSetColumnIndex(1);
                    if (this.iconManager.TryGetIcon(craftSkillData.IconIds[classIndex], out var textureWrap))
                    {
                        ImGuiEx.ScaledImageY(textureWrap.ImGuiHandle, textureWrap.Width, textureWrap.Height, ImGui.GetTextLineHeight());
                    }
                    else
                    {
                        ImGui.Dummy(new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted(craftSkillData.Name[this.ClientLanguage]);
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

            guiListClipperPtr.End();
            guiListClipperPtr.Destroy();

            ImGui.EndTable();
        }
    }

    private void DisplayQueueAndHistory()
    {
        const int columnCount = 8;

        void DrawQueueTable()
        {
            if (!ImGui.BeginTable("##queue-table", columnCount))
            {
                return;
            }

            DrawHeader();

            var currentJob = this.craftQueue.CurrentJob;
            if (currentJob != null)
            {
                DrawJobRow(currentJob, "current", "Cancel current Job", (Action)(() => this.craftQueue.CancelCurrentJob()));
            }

            var remove = -1;
            for (var i = 0; i < this.craftQueue.Jobs.Count; i++)
            {
                var job2 = this.craftQueue.Jobs[i];
                var index = i;
                var cancel = (Action)(() => remove = index);
                DrawJobRow(job2, "queued-" + i, $"Cancel queued Job {i}", cancel);
            }

            ImGui.EndTable();

            if (remove != -1)
            {
                this.craftQueue.DequeueJob(remove);
            }
        }

        void DrawHistoryTable()
        {
            if (!this.craftQueue.JobsCompleted.Any())
            {
                return;
            }

            if (!ImGui.BeginTable("##history-table", columnCount))
            {
                return;
            }

            DrawHeader();

            var remove = -1;
            for (var i = this.craftQueue.JobsCompleted.Count - 1; i > -1; --i)
            {
                var index = i;
                var removeAction = (Action)(() => remove = index);
                DrawJobRow(this.craftQueue.JobsCompleted[i], "history-" + i, $"Remove History Entry {i}", removeAction);
            }

            ImGui.EndTable();

            if (remove != -1)
            {
                this.craftQueue.DeleteHistory(remove);
            }
        }

        void DrawHeader()
        {
            ImGui.TableSetupColumn("Item", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Qty", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Step", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Duration", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Per craft", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##cancel", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();
        }

        void DrawJobRow(CraftingJob drawnJob, string? id, string tooltip, Action? cancel)
        {
            if (drawnJob.Status == JobStatus.Failure)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudRed);
            }

            ImGui.TableNextRow();
            var timeSpan1 = drawnJob.Duration;
            var timeSpan2 = drawnJob.LoopDuration;
            var str1 = drawnJob.Recipe.ResultItemName ?? "???";
            var str2 = this.classJobsSheet.GetRow(drawnJob.Recipe.Class.GetRowId())?.Abbreviation.RawString ?? "???";
            var values = new object[columnCount];
            values[0] = str1;
            values[1] = str2;
            values[2] = drawnJob.CurrentLoop.ToString() + '/' + drawnJob.Loops;
            values[3] = drawnJob.Status.ToString();
            values[4] = $"{drawnJob.CurrentStep} {drawnJob.RotationCurrentStep}/{drawnJob.RotationMaxSteps}";
            values[5] = timeSpan1.ToString("hh':'mm':'ss");
            values[6] = "~" + timeSpan2.ToString("mm':'ss");
            values[7] = (Action)(() =>
            {
                if (id == null || cancel == null || !ImGuiEx.IconButton(FontAwesomeIcon.Times, tooltip, small: true))
                {
                    return;
                }

                cancel();
            });
            ImGuiEx.TableRow(values, ImGui.IsItemHovered, index =>
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted("Rotation: " + drawnJob.RotationName);
                if (drawnJob.Food != null)
                {
                    ImGui.TextUnformatted(this.foodLabel + ": " + drawnJob.Food!.Name + (drawnJob.Food!.IsHq ? " \ue03c" : string.Empty));
                }

                if (drawnJob.Potion != null)
                {
                    ImGui.TextUnformatted(this.potionLabel + ": " + drawnJob.Potion!.Name + (drawnJob.Potion!.IsHq ? " \ue03c" : string.Empty));
                }

                ImGui.EndTooltip();
            });

            if (drawnJob.Status == JobStatus.Failure)
            {
                ImGui.PopStyleColor();
            }
        }

        using var raii = new ImGuiRaii();

        if (!raii.Begin(() => ImGui.BeginChild("##queue-and-history"), ImGui.EndChild))
        {
            return;
        }

        var queueState = this.craftQueue.Paused;
        var stateName = queueState switch
        {
            QueueState.Paused => "Paused",
            QueueState.Running => "Running",
            QueueState.PausedSoon => "Pausing Soon",
            _ => throw new ArgumentOutOfRangeException(),
        };

        var buttonCol = ImGuiEx.GetStyleColorVec4(ImGuiCol.Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, buttonCol);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, buttonCol);
        ImGui.Button($"{stateName}##LoopState", new Vector2(100, 0));
        ImGui.PopStyleColor();
        ImGui.PopStyleColor();

        switch (queueState)
        {
            case QueueState.Paused:
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Play, "Resume"))
                {
                    this.craftQueue.Paused = QueueState.Running;
                }

                break;

            case QueueState.Running:
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Pause, "Pause (hold control to pause at next craft)"))
                {
                    var io = ImGui.GetIO();
                    var ctrlHeld = io.KeyCtrl;
                    this.craftQueue.Paused = ctrlHeld ? QueueState.PausedSoon : QueueState.Paused;
                }

                break;
        }

        if (this.craftQueue.CurrentJob != null)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Cancel"))
            {
                this.craftQueue.CancelCurrentJob();
            }
        }

        if (!raii.Begin(() => ImGui.BeginChild("##queue-and-history-table-child", new Vector2(-1, ImGui.GetContentRegionAvail().Y)), ImGui.EndChild))
        {
            return;
        }

        DrawQueueTable();
        ImGui.Separator();
        DrawHistoryTable();
    }

    private void SetSimulation()
    {
        if (this.selectedRecipe is null)
        {
            return;
        }

        var job = this.selectedRecipe.Class.GetJob();

        var gs = this.gearsetManager.AllGearsets.FirstOrDefault(g => g.JobClass == job);
        if (gs == null)
        {
            return;
        }

        this.craftingSimulation = new Simulation(
            gs.ToCrafterStats(),
            this.selectedRecipe);
    }

    private void RunSimulation()
    {
        if (this.selectionChanged && this.craftingSimulation is not null)
        {
            this.craftingSimulation.CurrentStatModifiers = this.GetCurrentStatBuffs().ToArray();
            if (this.selectedRotation is not null)
            {
                this.simulationResult = this.craftingSimulation.Run(this.selectedRotation.Rotations, true);
            }
            else
            {
                this.craftingSimulation.Reset();
            }

            this.selectionChanged = false;
        }
    }

    private void FilterRecipes(string needle)
    {
        var recipes = this.craftQueueData.Recipes;
        if (string.IsNullOrEmpty(needle))
        {
            this.filteredRecipes.Clear();
            for (var index = 0; index < recipes.Count; ++index)
            {
                this.filteredRecipes.Add((index, recipes[index].Recipe, recipes[index].Job));
            }
        }
        else
        {
            needle = needle.ToLowerInvariant();
            this.filteredRecipes.Clear();
            for (var index = 0; index < recipes.Count; ++index)
            {
                var (recipe, job) = recipes[index];
                if (recipe.ItemResult.Value != null && recipe.ItemResult.Value.Name.RawString.ToLowerInvariant().Contains(needle))
                {
                    this.filteredRecipes.Add((index, recipe, job));
                }
            }
        }
    }

    private bool UpdateQueueChecklist([NotNullWhen(false)] out Action? action)
    {
        var ci = this.craftQueue.CommandInterface;

        var recipeSelected = this.recipeIdx > -1;
        var rotationSelected = this.selectedRotation is not null;
        var haveGearset = this.simulationResult?.Success == true;
        var haveFood = this.selectedFood is null || ci.CountItem(this.selectedFood.ItemId, this.selectedFood.IsHq) > 0;
        var havePotion = this.selectedPotion is null || ci.CountItem(this.selectedPotion.ItemId, this.selectedPotion.IsHq) > 0;
        var valid = recipeSelected & rotationSelected & haveGearset & this.haveAllIngredient & haveFood & havePotion;

        if (!valid)
        {
            action = () =>
            {
                ImGui.PushTextWrapPos();
                ImGui.BeginDisabled();
                ImGui.Checkbox("Recipe selected", ref recipeSelected);
                ImGui.Checkbox("Have ingredients (considering queue)", ref this.haveAllIngredient);
                ImGui.Spacing();
                ImGui.Checkbox("Rotation selected", ref rotationSelected);
                ImGui.Checkbox("Have gearset with required stats and rotation for success", ref haveGearset);
                ImGui.Checkbox("Have food", ref haveFood);
                ImGui.Checkbox("Have potion", ref havePotion);
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

    private void UpdateCurrentIngredients()
    {
        if (this.selectedRecipe is null)
        {
            this.currentIngredients = Array.Empty<CurrentIngredient>();
            return;
        }

        this.haveAllIngredient = true;

        var result = new CurrentIngredient[this.selectedRecipe.Ingredients.Length];

        for (var index = 0; index < result.Length; index++)
        {
            var ingredient = this.selectedRecipe.Ingredients[index];

            var itemId = ingredient.Id;

            var total = ingredient.Amount;

            ushort nqAmount = 0;
            var nqAvailable = this.ci.CountItem(itemId) - this.craftQueue.CountItemInQueueAndCurrent(itemId, false);
            if (nqAvailable > 0x100000)
            {
                nqAvailable = 0;
            }

            var hqIndex = -1;
            ushort hqAmount = 0;
            uint hqAvailable = 0;
            if (ingredient.CanBeHq && (hqIndex = Array.FindIndex(this.hqIngredients, i => i.ItemId == itemId)) != -1)
            {
                hqAmount = this.hqIngredients[hqIndex].Amount;
                nqAmount = (byte)(total - hqAmount);
                hqAvailable = this.ci.CountItem(itemId, true) - this.craftQueue.CountItemInQueueAndCurrent(itemId, true);
                if (hqAvailable > 0x100000)
                {
                    nqAvailable = 0;
                }
            }
            else
            {
                nqAmount = total;
            }

            var haveAllNq = true;
            var haveAllHq = true;
            if (nqAvailable < nqAmount * this.craftCount)
            {
                haveAllNq = false;
                this.haveAllIngredient = false;
            }

            if (hqIndex != -1 && hqAvailable < hqAmount * this.craftCount)
            {
                haveAllHq = false;
                this.haveAllIngredient = false;
            }

            result[index] = new CurrentIngredient((ItemId: itemId, ingredient.IconId, ingredient.Amount, NqCount: nqAmount, NqAvailable: nqAvailable, HaveAllNq: haveAllNq, HqIndex: hqIndex, HqCount: hqAmount, HqAvailable: hqAvailable, HaveAllHq: haveAllHq));
        }

        this.currentIngredients = result;
    }

    private IEnumerable<StatModifiers> GetCurrentStatBuffs()
    {
        if (this.foodIdx >= 0)
        {
            var food = this.craftQueueData.Foods[this.foodIdx];
            yield return food.Stats;
        }

        if (this.potionIdx >= 0)
        {
            var potion = this.craftQueueData.Potions[this.potionIdx];
            yield return potion.Stats;
        }
    }

    private void RotationSolver()
    {
        if (this.rotations is null || this.craftingSimulation is null)
        {
            return;
        }

        List<(RotationNode Node, SimulationResult Result)> success = new();
        foreach (var rotationNode in this.rotations)
        {
            var result = this.craftingSimulation.Run(rotationNode.Rotations, true);
            if (result.Success)
            {
                success.Add((rotationNode, result));
            }
        }

        if (success.Count == 0)
        {
            return;
        }

        int StepOrder((RotationNode Node, SimulationResult Result) s) => s.Result.Steps.Where(ar => !ar.Skipped).Sum(ar => ar.Skill.Action.GetWaitDuration());
        var winner = (this.rotationHq ? success.OrderByDescending(s => s.Result.HqPercent).ThenBy(StepOrder) : success.OrderBy(StepOrder))
           .FirstOrDefault();

        this.selectedRotation = winner.Node;
        this.rotationIdx = Array.IndexOf(this.rotations, this.selectedRotation);
        this.selectionChanged = true;
    }
}