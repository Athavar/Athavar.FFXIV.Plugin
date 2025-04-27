// <copyright file="BaseQueueTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.UI;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftQueue.Interfaces;
using Athavar.FFXIV.Plugin.CraftQueue.Job;
using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.CraftSimulator;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Interface.Colors;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Plugin.Services;
using ImGuiNET;
using Lumina.Excel;
using Lumina.Excel.Sheets;

internal abstract class BaseQueueTab : Tab
{
    private readonly string gearsetLabel;

    private Gearset[]? validGearsets;

    protected BaseQueueTab(CraftQueue craftQueue, ICraftDataManager craftDataManager, IIconManager iconManager)
    {
        var services = craftQueue.DalamudServices;
        this.CraftQueue = craftQueue;
        this.craftDataManager = craftDataManager;
        this.IconManager = iconManager;
        this.clientState = services.ClientState;
        this.gearsetManager = craftQueue.GearsetManager;

        var dataManager = services.DataManager;
        this.classJobsSheet = dataManager.GetExcelSheet<ClassJob>() ?? throw new AthavarPluginException();
        this.baseParamsSheet = dataManager.GetExcelSheet<BaseParam>() ?? throw new AthavarPluginException();
        this.addonsSheet = dataManager.GetExcelSheet<Addon>() ?? throw new AthavarPluginException();

        this.itemsSheet = dataManager.GetExcelSheet<Item>() ?? throw new AthavarPluginException();
        var sheet = dataManager.GetExcelSheet<ItemSearchCategory>();
        this.foodLabel = sheet?.GetRowOrDefault(45)?.Name.ToString() ?? string.Empty;
        this.potionLabel = sheet?.GetRowOrDefault(43)?.Name.ToString() ?? string.Empty;
        this.gearsetLabel = this.addonsSheet.GetRowOrDefault(756)?.Text.ToString() ?? string.Empty;
    }

    public override void OnNotDraw()
    {
        if (this.rotations is not null)
        {
            this.rotations = null;
            this.craftingSimulation = null;
            this.validGearsets = null;
        }
    }

    protected bool DisplayGearsetSelect()
    {
        [return: NotNullIfNotNull("gs")]
        string? GetName(Gearset? gs) => gs is null ? null : $"{gs.Id + 1} - {gs.Name}";

        if (this.validGearsets is null || this.validGearsets.Length <= 1)
        {
            return false;
        }

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

        return true;
    }

    protected void SetupSimulation()
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
        this.craftingSimulation.SetHqIngredients(this.hqIngredients);
        this.selectionChanged = true;
    }

    protected void RunSimulation(bool force = false)
    {
        if ((force || this.selectionChanged) && this.craftingSimulation is not null)
        {
            this.craftingSimulation.CurrentStatModifiers = this.GetCurrentStatBuffs().ToArray();
            if (this.selectedRotationResolver is IStaticRotationResolver staticRotationResolver)
            {
                this.simulationResult = this.craftingSimulation.Run(staticRotationResolver.Rotation, true);
                this.requiredCrafterDelineations = this.simulationResult.Steps.Count(s => s is { Success: true, Skill.Skill: CraftingSkills.HearthAndSoul or CraftingSkills.CarefulObservation or CraftingSkills.QuickInnovation });
            }
            else
            {
                this.craftingSimulation.Reset();
                this.simulationResult = null;
                this.requiredCrafterDelineations = 0;
            }

            this.selectionChanged = false;
        }
    }

    protected void DisplaySimulationResult()
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

        if (this.simulationResult is null || this.selectedRotationResolver is null)
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
            var areaWidth = ImGui.GetContentRegionAvail().X;
            var x = areaWidth;
            var size = ImGui.GetTextLineHeight() * 3;
            var itemWith = size + this.style.ItemSpacing.X;
            var classIndex = (int)this.selectedRecipe.Class;
            for (var index = 0; index < rotationSteps.Count; index++)
            {
                var actionResult = rotationSteps[index];
                var craftSkillData = this.craftDataManager.GetCraftSkillData(actionResult.Skill);
                var tex = this.IconManager.GetIcon(craftSkillData.IconIds[classIndex]);
                if (tex is null || !tex.TryGetWrap(out var texWarp, out _))
                {
                    continue;
                }

                var cursorBeforeImage = ImGui.GetCursorPos();
                var iconSize = new Vector2(size, size);
                ImGui.Image(texWarp.ImGuiHandle, iconSize);
                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.TextUnformatted(craftSkillData.Name[this.clientState.ClientLanguage] + (actionResult.FailCause is not null ? ": " + actionResult.FailCause : string.Empty));
                    ImGui.EndTooltip();
                }

                if (actionResult.FailCause is not null)
                {
                    tex = this.IconManager.GetIcon(61502);
                    if (tex != null && tex.TryGetWrap(out texWarp, out _))
                    {
                        ImGui.SameLine();
                        ImGui.SetCursorPos(cursorBeforeImage);
                        ImGui.Image(texWarp.ImGuiHandle, iconSize);
                    }
                }

                x -= itemWith;
                if (index != rotationSteps.Count - 1 && itemWith <= x)
                {
                    ImGui.SameLine();
                }
                else
                {
                    x = areaWidth;
                }
            }
        }
    }

    protected void DisplayCraftingSteps()
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
                    if (this.IconManager.TryGetIcon(craftSkillData.IconIds[classIndex], out var sharedImmediateTexture) && sharedImmediateTexture.TryGetWrap(out var textureWrap, out _))
                    {
                        ImGuiEx.ScaledImageY(textureWrap, ImGui.GetTextLineHeight());
                    }
                    else
                    {
                        ImGui.Dummy(new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                    }

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.TextUnformatted(craftSkillData.Name[this.clientState.ClientLanguage]);
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

    protected void DisplayFoodSelect() => this.DisplayItemPicker(this.foodLabel, "food", this.CraftQueue.Data.Foods, ref this.foodIdx, ref this.selectedFood, CraftingJobFlags.ForceFood);

    protected void DisplayPotionSelect() => this.DisplayItemPicker(this.potionLabel, "potion", this.CraftQueue.Data.Potions, ref this.potionIdx, ref this.selectedPotion, CraftingJobFlags.ForcePotion);

    protected void RotationSolver(bool hq)
    {
        if (this.rotations is null || this.craftingSimulation is null)
        {
            var logger = this.CraftQueue.DalamudServices.PluginLogger;
            logger.Information($"rotations:{this.rotations is null} | craftingSimulation:{this.craftingSimulation is null}");
            return;
        }

        List<(RotationNode Node, SimulationResult Result)> success = [];
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
            // reset state to last simulation.
            this.CraftQueue.DalamudServices.NotificationManager.AddNotification(
                new Notification
                {
                    Content = $"No Rotation found to craft {this.selectedRecipe?.ResultItemName}",
                    Title = "Craft Resolver Result",
                    ExtensionDurationSinceLastInterest = TimeSpan.FromSeconds(3),
                });
            this.RunSimulation(true);
            return;
        }

        int StepOrder((RotationNode Node, SimulationResult Result) s) => s.Result.Steps.Where(ar => !ar.Skipped).Sum(ar => ar.Skill.Action.GetWaitDuration());
        var winner = (hq ? success.OrderByDescending(s => s.Result.HqPercent).ThenBy(StepOrder) : success.OrderBy(StepOrder))
           .FirstOrDefault();

        this.selectedRotationResolver = new RotationNodeResolver(winner.Node);
        this.selectionChanged = true;
    }

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
                        var icon = this.IconManager.GetIcon(new GameIconLookup(buffInfo.IconId, buffInfo.IsHq));
                        if (icon != null && icon.TryGetWrap(out var iconWarp, out _))
                        {
                            ImGuiEx.ScaledImageY(iconWarp, ImGui.GetTextLineHeight());
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

    private IEnumerable<StatModifiers> GetCurrentStatBuffs()
    {
        var data = this.CraftQueue.Data;
        if (this.foodIdx >= 0)
        {
            var food = data.Foods[this.foodIdx];
            yield return food.Stats;
        }

        if (this.potionIdx >= 0)
        {
            var potion = data.Potions[this.potionIdx];
            yield return potion.Stats;
        }
    }
#pragma warning disable SA1401
    protected readonly CraftQueue CraftQueue;
    protected readonly IIconManager IconManager;
    private readonly ICraftDataManager craftDataManager;
    private readonly IClientState clientState;
    private readonly IGearsetManager gearsetManager;

    protected readonly ExcelSheet<ClassJob> classJobsSheet;
    protected readonly ExcelSheet<BaseParam> baseParamsSheet;
    protected readonly ExcelSheet<Addon> addonsSheet;
    protected readonly ExcelSheet<Item> itemsSheet;

    protected readonly string potionLabel;
    protected readonly string foodLabel;

    protected Simulation? craftingSimulation;
    protected SimulationResult? simulationResult;

    protected RecipeExtended? selectedRecipe;
    protected IRotationResolver? selectedRotationResolver;
    protected Gearset? selectedGearset;
    protected BuffInfo? selectedFood;
    protected BuffInfo? selectedPotion;

    protected RotationNode[]? rotations;

    protected int foodIdx = -1;
    protected int potionIdx = -1;
    protected CraftingJobFlags flags = CraftingJobFlags.None;

    protected (uint ItemId, byte Amount)[] hqIngredients = [];
    protected int requiredCrafterDelineations;

    protected bool selectionChanged;

    protected ImGuiStylePtr style;
#pragma warning restore SA1401
}