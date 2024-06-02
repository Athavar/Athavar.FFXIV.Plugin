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
using Athavar.FFXIV.Plugin.CraftQueue.Extension;
using Athavar.FFXIV.Plugin.CraftQueue.Job;
using Athavar.FFXIV.Plugin.CraftSimulator;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;
using Action = Action;
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
    private readonly string gearsetLabel;
    private readonly string potionLabel;
    private readonly string foodLabel;
    private readonly List<(int Index, Recipe Recipe, Job Job)> filteredRecipes = new();

    private Gearset[]? validGearsets;

    private int craftCount = 1;

    private RotationNode[]? rotations;

    private int recipeIdx = -1;
    private string recipeSearch = string.Empty;

    private int rotationIdx = -1;
    private bool rotationHq;

    private int foodIdx = -1;
    private int potionIdx = -1;
    private CraftingJobFlags flags = CraftingJobFlags.None;

    private bool selectionChanged;

    private RecipeExtended? selectedRecipe;
    private Gearset? selectedGearset;
    private RotationNode? selectedRotation;
    private BuffInfo? selectedFood;
    private BuffInfo? selectedPotion;
    private (uint ItemId, byte Amount)[] hqIngredients = Array.Empty<(uint ItemId, byte Amount)>();
    private CurrentIngredient[] currentIngredients = Array.Empty<CurrentIngredient>();
    private bool haveAllIngredient;

    private Simulation? craftingSimulation;
    private SimulationResult? simulationResult;
    private int requiredCrafterDelineations;

    private bool init;
    private uint updateTick;

    private ImGuiStylePtr style;

    public QueueTab(CraftQueue craftQueue, IIconManager iconManager, ICraftDataManager craftDataManager, CraftQueueConfiguration configuration, ClientLanguage clientLanguage)
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
        this.gearsetLabel = this.addonsSheet.GetRow(756)?.Text ?? string.Empty;
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
        this.style = ImGui.GetStyle();
        if (!this.init)
        {
            this.init = true;
            this.PopulateData();
        }

        if (!this.craftQueue.DalamudServices.ClientState.IsLoggedIn)
        {
            ImGui.TextUnformatted("Please login.");
            this.rotations = null;
            return;
        }

        // tab is open. rotations is set to null if tab is not visible or on first run.
        if (this.rotations is null)
        {
            // refresh data that could have changed.
            this.rotations = this.Configuration.GetAllNodes().Where(node => node is RotationNode).Cast<RotationNode>().ToArray();
            this.rotationIdx = Array.IndexOf(this.rotations, this.selectedRotation);
            this.SetupSimulation();
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
            this.craftingSimulation = null;
            this.validGearsets = null;
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
            if (this.validGearsets is not null && this.validGearsets.Length > 1)
            {
                this.DisplayGearsetSelect();
                ImGui.Separator();
            }

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

                ImGuiClip.ClippedDraw(
                    this.filteredRecipes,
                    filteredRecipe =>
                    {
                        ImGui.TableNextRow();
                        var (num, recipe, job) = filteredRecipe;
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
                            this.SetupSimulation();
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
                    },
                    ImGui.GetTextLineHeightWithSpacing());

                ImGui.EndTable();
                ImGui.EndChild();
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayGearsetSelect()
    {
        [return: NotNullIfNotNull("gs")]
        string? GetName(Gearset? gs) => gs is null ? null : $"{gs.Id + 1} - {gs.Name}";

        var previewValue = GetName(this.selectedGearset) ?? "Select a gearset";
        if (ImGui.BeginCombo(this.gearsetLabel + "##cq-gearset-picker", previewValue))
        {
            for (var index = 0; index < this.validGearsets?.Length; ++index)
            {
                var gearset = this.validGearsets[index];
                if (ImGui.Selectable($"{GetName(gearset)}##cq-gearset-{index}", gearset == this.selectedGearset))
                {
                    this.selectedGearset = gearset;
                    this.selectionChanged = true;
                    this.SetupSimulation();
                }
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayFoodSelect() => this.DisplayItemPicker(this.foodLabel, "food", this.craftQueueData.Foods, ref this.foodIdx, ref this.selectedFood, CraftingJobFlags.ForceFood);

    private void DisplayPotionSelect() => this.DisplayItemPicker(this.potionLabel, "potion", this.craftQueueData.Potions, ref this.potionIdx, ref this.selectedPotion, CraftingJobFlags.ForcePotion);

    private void DisplayItemPicker(
        string label,
        string id,
        IReadOnlyList<BuffInfo> items,
        ref int idx,
        ref BuffInfo? selected,
        CraftingJobFlags forcedFlag)
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

                // copy ref to local variables
                var selIndex = idx;
                var selItem = selected;

                ImGuiClip.ClippedDraw(
                    items,
                    (buffInfo, index) =>
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        var cursorPos1 = ImGui.GetCursorPos();
                        if (ImGui.Selectable($"##{index}", selIndex == index, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            selIndex = index;
                            selItem = items[index];
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
                    },
                    ImGui.GetTextLineHeightWithSpacing());

                // restore local variables to ref
                selected = selItem;
                idx = selIndex;

                ImGui.EndTable();
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();

        const string text = "force";
        var val = (this.flags & forcedFlag) != 0;
        if (ImGui.Checkbox(text + "##forced-checkbox-" + id, ref val))
        {
            this.flags ^= forcedFlag;
        }

        ImGuiEx.TextTooltip($"Force the usage of {label}");
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

        if (ImGui.Button("Queue##queue-btn") && this.selectedRecipe is not null && this.selectedGearset is not null && this.selectedRotation is not null)
        {
            this.craftQueue.CreateJob(this.selectedRecipe, this.selectedGearset, this.selectedRotation, (uint)this.craftCount, this.selectedFood, this.selectedPotion, this.hqIngredients, this.flags);
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
                var ingredient = this.currentIngredients[index];
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

                var cursorBeforeImage = ImGui.GetCursorPos();
                ImGui.Image(tex.ImGuiHandle, iconSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(craftSkillData.Name[this.ClientLanguage] + (actionResult.FailCause is not null ? ": " + actionResult.FailCause : string.Empty));
                    ImGui.EndTooltip();
                }

                if (actionResult.FailCause is not null)
                {
                    tex = this.iconManager.GetIcon(61502, ITextureProvider.IconFlags.HiRes);
                    if (tex != null)
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPos(cursorBeforeImage);
                        ImGui.Image(tex.ImGuiHandle, iconSize);
                    }
                }

                if (index != rotationSteps.Count - 1 && 80.0 + this.style.ItemSpacing.X <= x)
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
        var maxRows = rotationSteps.Count + 1;
        var yAvail = ImGui.GetContentRegionAvail().Y;
        var height = ImGui.GetTextLineHeightWithSpacing() * Math.Min(4, maxRows);
        var maxHeight = maxRows * ImGui.GetTextLineHeightWithSpacing();
        if (ImGui.CollapsingHeader("Steps##step-table-collapsing") && ImGui.BeginTable("##step-table", 6, ImGuiTableFlags.ScrollY | ImGuiTableFlags.Borders, new Vector2(0.0f, yAvail < height ? height : yAvail > maxHeight + ImGui.GetTextLineHeightWithSpacing() ? maxHeight : 0.0f)))
        {
            ImGui.TableSetupColumn("Step##index", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("##icon", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("CP##cp", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Durability##durability", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Quality##quality", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn("Progress##progress", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            var classIndex = (int)this.selectedRecipe.Class;
            ImGuiClip.ClippedDraw(
                rotationSteps,
                (step, index) =>
                {
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
                },
                ImGui.GetTextLineHeightWithSpacing());

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

        void DrawJobRow(BaseCraftingJob drawnJob, string? id, string tooltip, Action? cancel)
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
            ImGuiEx.TableRow(
                values,
                ImGui.IsItemHovered,
                index =>
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted("Rotation: " + drawnJob.RotationName);
                    if (drawnJob.BuffConfig.Food is { } food)
                    {
                        ImGui.TextUnformatted(this.foodLabel + ": " + food.Name + (food.IsHq ? " \ue03c" : string.Empty));
                    }

                    if (drawnJob.BuffConfig.Potion is { } potion)
                    {
                        ImGui.TextUnformatted(this.potionLabel + ": " + potion.Name + (potion.IsHq ? " \ue03c" : string.Empty));
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
                    this.craftQueue.Start();
                }

                break;

            case QueueState.Running:
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Pause, "Pause (hold control to pause at next craft)"))
                {
                    var io = ImGui.GetIO();
                    var ctrlHeld = io.KeyCtrl;
                    this.craftQueue.Pause(ctrlHeld);
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

    private void SetupSimulation()
    {
        void Reset()
        {
            SimReset();
            this.selectedGearset = null;
        }

        void SimReset()
        {
            this.simulationResult = null;
            this.craftingSimulation = null;
        }


        if (this.selectedRecipe is null)
        {
            Reset();
            return;
        }

        var job = this.selectedRecipe.Class.GetJob();
        this.validGearsets = this.gearsetManager.AllGearsets.Where(g => g.JobClass == job).ToArray();
        if (this.validGearsets.Length == 0)
        {
            Reset();
            return;
        }

        Gearset? foundMatch = null;
        if (this.selectedGearset is null || (foundMatch = this.validGearsets.FirstOrDefault(g => g.Id == this.selectedGearset.Id)) == null)
        {
            // set gearset if null or invalid for current recipe.
            this.selectedGearset = this.validGearsets.First();
        }
        else
        {
            this.selectedGearset = foundMatch;
        }

        if (this.selectedGearset == null)
        {
            SimReset();
            return;
        }

        this.craftingSimulation = new Simulation(
            this.selectedGearset.ToCrafterStats(),
            this.selectedRecipe);
        this.selectionChanged = true;
    }

    private void RunSimulation()
    {
        if (this.selectionChanged && this.craftingSimulation is not null)
        {
            this.craftingSimulation.CurrentStatModifiers = this.GetCurrentStatBuffs().ToArray();
            if (this.selectedRotation is not null)
            {
                this.simulationResult = this.craftingSimulation.Run(this.selectedRotation.Rotations, true);
                this.requiredCrafterDelineations = this.simulationResult.Steps.Count(s => s is { Success: true, Skill.Skill: CraftingSkills.HearthAndSoul or CraftingSkills.CarefulObservation });
            }
            else
            {
                this.craftingSimulation.Reset();
                this.requiredCrafterDelineations = 0;
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
        if (this.selectedRecipe is null || this.selectedGearset is null)
        {
            action = () => { };
            return false;
        }

        var ci = this.craftQueue.CommandInterface;

        var recipeSelected = this.recipeIdx > -1;
        var rotationSelected = this.selectedRotation is not null;
        var haveGearset = this.simulationResult?.Success == true;
        var haveReqItem = this.selectedRecipe.ItemReq is null || this.selectedGearset.ItemIds.Contains(this.selectedRecipe.ItemReq.GetValueOrDefault());
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

                if (this.selectedRecipe.ItemReq is not null)
                {
                    ImGui.Checkbox($"Have {this.selectedRecipe.RequiredItemName} equipped", ref haveReqItem);
                }

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

        var ingredientsCount = this.selectedRecipe.Ingredients.Length;
        var resultCount = ingredientsCount + (this.requiredCrafterDelineations > 0 ? 1 : 0);
        var result = new CurrentIngredient[resultCount];
        if (this.requiredCrafterDelineations > 0)
        {
            // Crafter's Delineation
            const uint itemId = 28724;
            const ushort iconId = 26188;
            var amount = (byte)this.requiredCrafterDelineations;
            var available = this.ci.CountItem(itemId) - this.craftQueue.CountItemInQueueAndCurrent(itemId, false);
            var haveAll = available >= amount * this.craftCount;
            result[resultCount - 1] = new CurrentIngredient(itemId, iconId, amount, amount, available, haveAll, -1, 0, 0, true);
            this.haveAllIngredient = haveAll;
        }

        for (var index = 0; index < ingredientsCount; index++)
        {
            var ingredient = this.selectedRecipe.Ingredients[index];

            var itemId = ingredient.Id;

            var total = ingredient.Amount;

            ushort nqAmount;
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

            result[index] = new CurrentIngredient(itemId, ingredient.IconId, ingredient.Amount, nqAmount, nqAvailable, haveAllNq, hqIndex, hqAmount, hqAvailable, haveAllHq);
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

    private sealed record CurrentIngredient(uint ItemId, ushort Icon, byte Amount, ushort NqCount, uint NqAvailable, bool HaveAllNq, int HqIndex, ushort HqCount, uint HqAvailable, bool HaveAllHq);
}