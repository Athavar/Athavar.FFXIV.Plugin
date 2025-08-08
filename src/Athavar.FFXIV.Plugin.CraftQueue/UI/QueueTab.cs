// <copyright file="QueueTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.CraftQueue.Extension;
using Athavar.FFXIV.Plugin.CraftQueue.Interfaces;
using Athavar.FFXIV.Plugin.CraftQueue.Job;
using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Utility;
using Action = Action;
using Recipe = Lumina.Excel.Sheets.Recipe;

internal sealed class QueueTab : BaseQueueTab
{
    internal const string Id = "Tab-CQQueue";

    private readonly CraftQueueData craftQueueData;
    private readonly ICommandInterface ci;

    private readonly List<(int Index, Recipe Recipe, Job Job)> filteredRecipes = [];

    private int craftCount = 1;

    private int recipeIdx = -1;
    private string recipeSearch = string.Empty;

    private bool rotationHq;

    private CurrentIngredient[] currentIngredients = [];
    private bool haveAllIngredient;

    private bool init;
    private uint updateTick;

    public QueueTab(CraftQueue craftQueue, IIconManager iconManager, ICraftDataManager craftDataManager, CraftQueueConfiguration configuration)
        : base(craftQueue, craftDataManager, iconManager)
    {
        this.craftQueueData = craftQueue.Data;
        this.ci = craftQueue.CommandInterface;
        this.Configuration = configuration;
    }

    /// <inheritdoc/>
    public override string Name => "Queue";

    /// <inheritdoc/>
    public override string Identifier => Id;

    private CraftQueueConfiguration Configuration { get; }

    /// <inheritdoc/>
    public override void Draw()
    {
        this.style = ImGui.GetStyle();
        if (!this.init)
        {
            this.init = true;
            this.PopulateData();
        }

        if (!this.CraftQueue.DalamudServices.ClientState.IsLoggedIn)
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
            this.SetupSimulation();
        }

        this.updateTick++;
        if (this.updateTick > 10)
        {
            this.UpdateCurrentIngredients();
            this.updateTick = 0;

            if (this.selectedRotationResolver is { } rotationResolver)
            {
                var name = rotationResolver.Name;

                // check selected rotation exists
                if (this.CraftQueue.ExternalResolver.All(r => r.Name != name) && this.rotations.All(r => r.Name != name))
                {
                    // reset selection.
                    this.selectionChanged = true;
                    this.selectedRotationResolver = null;
                }
            }
        }

        ImGui.Columns(2);

        this.DisplayAddQueueItem();

        ImGui.NextColumn();

        this.DisplayQueueAndHistory();

        ImGui.Columns();
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
            if (this.DisplayGearsetSelect())
            {
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
            this.RunSimulation(this.rotationHq);
            if (this.selectedRotationResolver is IStaticRotationResolver)
            {
                // only static resolver can be simulated.
                this.DisplaySimulationResult();
                this.DisplayCraftingSteps();
            }

            ImGui.EndChild();
        }
    }

    private void DisplayRecipeSelect()
    {
        ImGui.SetNextItemWidth(-1f);
        if (ImGui.BeginCombo("##recipe-list", this.recipeIdx > -1 ? this.craftQueueData.Recipes[this.recipeIdx].Recipe.ItemResult.ValueNullable?.Name.ToDalamudString().TextValue ?? "???" : "Select a recipe", ImGuiComboFlags.HeightLargest))
        {
            ImGui.SetNextItemWidth(-1f);
            if (ImGui.InputTextWithHint("##recipe-search", "Search...", ref this.recipeSearch, 512, ImGuiInputTextFlags.AutoSelectAll))
            {
                this.recipeSearch = this.recipeSearch.Trim();
                this.FilterRecipes(this.recipeSearch);
            }

            if (!ImGui.IsAnyItemFocused() && !ImGui.IsAnyItemActive() && !ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                ImGui.SetKeyboardFocusHere(-1);
            }

            if (ImGui.BeginTable("##cq-recipe-list-table", 4, ImGuiTableFlags.ScrollY, new Vector2(0.0f, 250f) * ImGuiHelpers.GlobalScale))
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
                        var text = this.classJobsSheet.GetRowOrDefault((uint)job)?.Abbreviation.ExtractText() ?? "???";
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

                        var obj = recipe.ItemResult.ValueNullable;
                        if (obj is not null && this.IconManager.TryGetIcon(obj.Value.Icon, out var texture) && texture.TryGetWrap(out var textureWrap, out _))
                        {
                            ImGui.SetCursorPos(cursorPos);
                            ImGuiEx.ScaledImageY(textureWrap, ImGui.GetTextLineHeight());
                        }
                        else
                        {
                            ImGui.Dummy(new Vector2(ImGui.GetTextLineHeight(), ImGui.GetTextLineHeight()));
                        }

                        ImGui.TableSetColumnIndex(1);
                        ImGui.TextUnformatted(text);
                        ImGui.TableSetColumnIndex(2);
                        ImGui.TextUnformatted($"{recipe.RecipeLevelTable.RowId}");
                        ImGui.TableSetColumnIndex(3);
                        ImGui.TextUnformatted(obj.GetValueOrDefault().Name.ToDalamudString().TextValue);
                    },
                    ImGui.GetTextLineHeightWithSpacing());

                ImGui.EndTable();
            }

            ImGui.EndCombo();
        }
    }

    private void DisplayRotationSelect()
    {
        var previewValue = this.selectedRotationResolver?.Name ?? "Select a rotation";
        if (ImGui.BeginCombo("##cq-rotation-picker", previewValue))
        {
            ImGuiEx.TextTooltip("Try to find rotation with the best HQ percentage");

            var externalResolver = this.CraftQueue.ExternalResolver;
            for (var index = 0; index < externalResolver.Count; index++)
            {
                var resolver = externalResolver[index];
                if (ImGui.Selectable($"{resolver.Name}##cq-ex-rotation-{index}", resolver.Name == this.selectedRotationResolver?.Name))
                {
                    this.selectedRotationResolver = resolver;
                    this.selectionChanged = true;
                }
            }

            for (var index = 0; index < this.rotations?.Length; ++index)
            {
                var rotation = this.rotations[index];
                if (ImGui.Selectable($"{rotation.Name}##cq-rotation-{index}", rotation.Name == this.selectedRotationResolver?.Name))
                {
                    this.selectedRotationResolver = new RotationNodeResolver(rotation);
                    this.selectionChanged = true;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.Button("Recommend##cq-rotation-picker-find"))
        {
            this.RotationSolver(this.rotationHq);
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

        var checkResult = this.UpdateQueueChecklist(out var drawChecklistAction);
        var io = ImGui.GetIO();
        var ctrlHeld = io.KeyCtrl;

        var valid = checkResult.ValidCraftRequirements && (checkResult.HasIngredients || ctrlHeld);
        if (!valid)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Queue##queue-btn") && this.selectedRecipe is not null && this.selectedGearset is not null && this.selectedRotationResolver is not null)
        {
            var flags = this.flags;
            if (ctrlHeld || !valid)
            {
                flags |= CraftingJobFlags.TrialSynthesis;
            }

            this.CraftQueue.CreateJob(this.selectedRecipe, this.selectedGearset, this.selectedRotationResolver, (uint)this.craftCount, this.selectedFood, this.selectedPotion, this.hqIngredients, flags);
        }

        if (!valid)
        {
            ImGui.EndDisabled();
        }

        if (checkResult.ValidCraftRequirements)
        {
            ImGuiEx.TextTooltip(ctrlHeld ? "trial synthesis" : "hold control to queue as trial synthesis", ImGuiHoveredFlags.AllowWhenDisabled);
        }

        if (!checkResult.ValidCraftRequirements || !checkResult.HasIngredients)
        {
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
                    if (ImGui.TableSetColumnIndex(0) && this.IconManager.TryGetIcon(ingredient.Icon, out var texture) && texture.TryGetWrap(out var textureWarp, out _))
                    {
                        ImGuiEx.ScaledImageY(textureWarp.Handle, textureWarp.Width, textureWarp.Height, ImGui.GetTextLineHeight());
                        ImGuiEx.TextTooltip(this.itemsSheet.GetRowOrDefault(ingredient.ItemId)?.Name.ToDalamudString().TextValue ?? string.Empty);
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

            var currentJob = this.CraftQueue.CurrentJob;
            if (currentJob != null)
            {
                DrawJobRow(currentJob, "current", "Cancel current Job", (Action)(() => this.CraftQueue.CancelCurrentJob()));
            }

            var remove = -1;
            for (var i = 0; i < this.CraftQueue.Jobs.Count; i++)
            {
                var job2 = this.CraftQueue.Jobs[i];
                var index = i;
                var cancel = (Action)(() => remove = index);
                DrawJobRow(job2, "queued-" + i, $"Cancel queued Job {i}", cancel);
            }

            ImGui.EndTable();

            if (remove != -1)
            {
                this.CraftQueue.DequeueJob(remove);
            }
        }

        void DrawHistoryTable()
        {
            if (!this.CraftQueue.JobsCompleted.Any())
            {
                return;
            }

            if (!ImGui.BeginTable("##history-table", columnCount))
            {
                return;
            }

            DrawHeader();

            var remove = -1;
            for (var i = this.CraftQueue.JobsCompleted.Count - 1; i > -1; --i)
            {
                var index = i;
                var removeAction = (Action)(() => remove = index);
                DrawJobRow(this.CraftQueue.JobsCompleted[i], "history-" + i, $"Remove History Entry {i}", removeAction);
            }

            ImGui.EndTable();

            if (remove != -1)
            {
                this.CraftQueue.DeleteHistory(remove);
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
            var str2 = this.classJobsSheet.GetRowOrDefault(drawnJob.Recipe.Class.GetRowId())?.Abbreviation.ExtractText() ?? "???";
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

        var queueState = this.CraftQueue.Paused;
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
                    this.CraftQueue.Start();
                }

                break;

            case QueueState.Running:
                ImGui.SameLine();
                if (ImGuiEx.IconButton(FontAwesomeIcon.Pause, "Pause (hold control to pause at next craft)"))
                {
                    var io = ImGui.GetIO();
                    var ctrlHeld = io.KeyCtrl;
                    this.CraftQueue.Pause(ctrlHeld);
                }

                break;
        }

        if (this.CraftQueue.CurrentJob != null)
        {
            ImGui.SameLine();
            if (ImGuiEx.IconButton(FontAwesomeIcon.TrashAlt, "Cancel"))
            {
                this.CraftQueue.CancelCurrentJob();
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
                if (recipe.ItemResult.ValueNullable is { } itemResult && itemResult.Name.ExtractText().ToLowerInvariant().Contains(needle))
                {
                    this.filteredRecipes.Add((index, recipe, job));
                }
            }
        }
    }

    private (bool ValidCraftRequirements, bool HasIngredients) UpdateQueueChecklist([NotNullWhen(false)] out Action? action)
    {
        if (this.selectedRecipe is null || this.selectedGearset is null)
        {
            action = () => { };
            return (false, false);
        }

        var ci = this.CraftQueue.CommandInterface;

        var recipeSelected = this.recipeIdx > -1;
        var rotationSelected = this.selectedRotationResolver is not null;
        var haveGearset = this.selectedRotationResolver?.ResolverType == ResolverType.Dynamic || this.simulationResult?.Success == true;
        var haveReqItem = this.selectedRecipe.ItemReq is null || this.selectedGearset.ItemIds.Contains(this.selectedRecipe.ItemReq.GetValueOrDefault());
        var haveFood = this.selectedFood is null || ci.CountItem(this.selectedFood.ItemId, this.selectedFood.IsHq) > 0;
        var havePotion = this.selectedPotion is null || ci.CountItem(this.selectedPotion.ItemId, this.selectedPotion.IsHq) > 0;

        var validCraft = recipeSelected & rotationSelected & haveGearset;
        var validIngredients = this.haveAllIngredient & haveFood & havePotion;

        var valid = validCraft & validIngredients;

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

        return (validCraft, validIngredients);
    }

    private void UpdateCurrentIngredients()
    {
        if (this.selectedRecipe is null)
        {
            this.currentIngredients = [];
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
            var available = this.ci.CountItem(itemId) - this.CraftQueue.CountItemInQueueAndCurrent(itemId, false);
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
            var nqAvailable = this.ci.CountItem(itemId) - this.CraftQueue.CountItemInQueueAndCurrent(itemId, false);
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
                hqAvailable = this.ci.CountItem(itemId, true) - this.CraftQueue.CountItemInQueueAndCurrent(itemId, true);
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

    private sealed record CurrentIngredient(uint ItemId, ushort Icon, byte Amount, ushort NqCount, uint NqAvailable, bool HaveAllNq, int HqIndex, ushort HqCount, uint HqAvailable, bool HaveAllHq);
}