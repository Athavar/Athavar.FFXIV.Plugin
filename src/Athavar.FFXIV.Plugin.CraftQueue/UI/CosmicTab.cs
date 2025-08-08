// <copyright file="CosmicTab.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftQueue.UI;

using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftQueue.Extension;
using Athavar.FFXIV.Plugin.CraftQueue.Interfaces;
using Athavar.FFXIV.Plugin.CraftQueue.Job;
using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using FFXIVClientStructs.FFXIV.Client.Game.WKS;
using Lumina.Excel;
using Lumina.Excel.Sheets;

internal sealed class CosmicTab : BaseQueueTab
{
    public const string Id = "Tab-Cosmic";

    private readonly ExcelSheet<WKSMissionUnit> missionUnitSheet;

    private bool autoStart;

    private string currentMission = string.Empty;
    private ushort lastMissionUnitRowId;

    private CraftDataInfo? nextCraftDataInfo;

    public CosmicTab(CraftQueue craftQueue, ICraftDataManager craftDataManager, IIconManager iconManager)
        : base(craftQueue, craftDataManager, iconManager)
    {
        var dataManager = this.CraftQueue.DalamudServices.DataManager;
        this.missionUnitSheet = dataManager.GetExcelSheet<WKSMissionUnit>();
        this.CraftQueue.DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostSetup, Constants.Addons.WksRecipeNote, this.WksRecipeNodePostSetup);
        this.CraftQueue.DalamudServices.AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, Constants.Addons.WksRecipeNote, this.WksRecipeNodePostUpdate);
    }

    public override string Name => "Cosmic";

    public override string Identifier => Id;

    private unsafe ushort CurrentMissionsUnitRowId => WKSManager.Instance()->CurrentMissionUnitRowId;

    public override void Draw()
    {
        this.style = ImGui.GetStyle();
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
            this.rotations = this.CraftQueue.Configuration.GetAllNodes().Where(node => node is RotationNode).Cast<RotationNode>().ToArray();
            this.SetupSimulation();
        }

        this.CheckAndSetMission();
        this.RunSimulation();

        var disabled = this.selectedRecipe is null || this.selectedRotationResolver is null || this.CraftQueue.CurrentJob is not null;
        if (disabled)
        {
            ImGui.BeginDisabled();
        }

        if (ImGui.Button("Craft"))
        {
            this.StartJob(true);
        }

        if (disabled)
        {
            ImGui.EndDisabled();
        }

        ImGui.SameLine();

        if (ImGui.Checkbox("Autostart##cq-autstart", ref this.autoStart))
        {
        }

        ImGui.Separator();
        ImGui.Text("Current Mission: ");
        ImGui.SameLine();
        ImGui.Text(this.currentMission);

        if (this.CurrentMissionsUnitRowId <= 0)
        {
            return;
        }

        ImGui.Text("Next Item: ");
        ImGui.SameLine();
        ImGui.Text(this.selectedRecipe?.ResultItemName ?? string.Empty);

        ImGui.Separator();

        this.DisplayRotationSelect();
        this.DisplayFoodSelect();
        this.DisplayPotionSelect();

        if (this.selectedRotationResolver is IStaticRotationResolver)
        {
            ImGui.Separator();

            // only static resolver can be simulated.
            this.DisplaySimulationResult();
            this.DisplayCraftingSteps();
        }
    }

    public override void Dispose()
    {
        this.CraftQueue.DalamudServices.AddonLifecycle.UnregisterListener(AddonEvent.PostUpdate, Constants.Addons.WksRecipeNote, this.WksRecipeNodePostUpdate);
        this.CraftQueue.DalamudServices.AddonLifecycle.UnregisterListener(AddonEvent.PostSetup, Constants.Addons.WksRecipeNote, this.WksRecipeNodePostSetup);
        base.Dispose();
    }

    private void StartJob(bool force = false)
    {
        if (this.selectedRecipe is null || this.selectedGearset is null || this.selectedRotationResolver is null)
        {
            return;
        }

        if (this.nextCraftDataInfo is not { } nextCraftData || this.CraftQueue.CurrentJob != null)
        {
            return;
        }

        if (!force)
        {
            var currentItems = this.CraftQueue.CommandInterface.CountItem(nextCraftData.ItemId) + this.CraftQueue.CommandInterface.CountItem(nextCraftData.ItemId, true);
            if (currentItems >= nextCraftData.Required)
            {
                this.nextCraftDataInfo = null;
                return;
            }
        }

        var flags = this.flags;
        flags |= CraftingJobFlags.CosmicSynthesis;
        this.CraftQueue.Clear();
        this.CraftQueue.CreateJob(this.selectedRecipe, this.selectedGearset, this.selectedRotationResolver, 1, this.selectedFood, this.selectedPotion, this.hqIngredients, flags);
        this.CraftQueue.Start();
    }

    private CraftDataInfo? NextCraftData(ushort missionUnitRowId)
    {
        var logger = this.CraftQueue.DalamudServices.PluginLogger;
        void Reset()
        {
            this.selectedRecipe = null;
            missionUnitRowId = 0;
        }

        var dataManager = this.CraftQueue.DalamudServices.DataManager;
        var missionsUnit = this.missionUnitSheet.GetRow(missionUnitRowId);
        foreach (var missionsTodo in missionsUnit.MissionToDo.Select(row => row.Value))
        {
            var wksItemSheet = dataManager.GetExcelSheet<WKSItemInfo>();

            var missionRecipeRowId = missionsUnit.WKSMissionRecipe;
            if (!missionRecipeRowId.IsValid)
            {
                Reset();
                return null;
            }

            var recipes = missionRecipeRowId
               .Value
               .Recipe
               .Where(r => r.IsValid)
               .Select(recipeRef => recipeRef.Value)
               .ToList();

            CraftDataInfo? NextCraft(Item item, uint quantity, bool requireHq)
            {
                var itemId = item.RowId;
                var currentNq = requireHq is false ? this.CraftQueue.CommandInterface.CountItem(itemId) : 0;
                var currentHq = this.CraftQueue.CommandInterface.CountItem(itemId, true);
                var currentItems = currentNq + currentHq;
                if (currentItems >= quantity)
                {
                    // full
                    return null;
                }

                var remainingQuantity = quantity - currentItems;
                var recipe = recipes.Find(i => i.ItemResult.RowId == itemId);
                if (recipe.RowId == 0)
                {
                    return null;
                }

                logger.Information($"NextCraft -> {itemId}:{item.Name}, Need: {quantity}, Found: {currentItems}, Missing: {remainingQuantity}, HQ?:{requireHq}");

                var ingredients = recipe.Ingredient.Zip(recipe.AmountIngredient).ToArray();
                foreach (var (rowRef, amount) in ingredients)
                {
                    if (amount <= 0)
                    {
                        continue;
                    }

                    if (NextCraft(this.itemsSheet.GetRow(rowRef.RowId), remainingQuantity * amount, recipe.Unknown1) is { } dataInfo)
                    {
                        return dataInfo;
                    }
                }

                return new CraftDataInfo(recipe, itemId, quantity, ingredients.Where(i => i.Second != 0).Select(i => (i.First.RowId, i.Second)).ToArray(), requireHq);
            }

            foreach (var (wksItemRef, quantity) in missionsTodo.RequiredItem.Zip(missionsTodo.RequiredItemQuantity))
            {
                var itemId = wksItemRef.Value.Item;
                var nextItem = itemId.Value;
                if (NextCraft(nextItem, quantity, false) is { } dataInfo)
                {
                    return dataInfo;
                }
            }
        }

        return null;
    }

    private void WksRecipeNodePostSetup(AddonEvent type, AddonArgs args)
    {
        this.CheckAndSetMission();
    }

    private void WksRecipeNodePostUpdate(AddonEvent type, AddonArgs args)
    {
        if (this.CurrentMissionsUnitRowId <= 0)
        {
            return;
        }

        if (this.nextCraftDataInfo is not { } nextCraftData)
        {
            if (this.NextCraftData(this.CurrentMissionsUnitRowId) is { } craftDataInfo)
            {
                this.nextCraftDataInfo = craftDataInfo;
                this.selectedRecipe = craftDataInfo.Recipe.ToCraftSimulatorRecipe(this.itemsSheet);
                this.hqIngredients = craftDataInfo
                   .Ingredient
                   .Select(item => item with
                    {
                        Amount = Math.Min(item.Amount, (byte)this.CraftQueue.CommandInterface.CountItem(item.ItemId, true)),
                    })
                   .Where(i => i.Amount != 0)
                   .ToArray();
                this.SetupSimulation();
                this.RotationSolver(true);
                this.selectionChanged = true;
            }

            // finish crafting
            return;
        }

        var currentNq = nextCraftData.RequireHq is false ? this.CraftQueue.CommandInterface.CountItem(nextCraftData.ItemId) : 0;
        var currentHq = this.CraftQueue.CommandInterface.CountItem(nextCraftData.ItemId, true);
        if (currentNq + currentHq >= nextCraftData.Required)
        {
            // finish crafting with this step.
            this.nextCraftDataInfo = null;
            return;
        }

        if (this.CraftQueue.CurrentJob is null && this.simulationResult?.Success == true && this.autoStart)
        {
            var logger = this.CraftQueue.DalamudServices.PluginLogger;
            logger.Debug($"Start Job {nextCraftData.Recipe.ItemResult.Value.Name}:{nextCraftData.Required}");
            this.StartJob();
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
            this.RotationSolver(true);
        }
    }

    private void CheckAndSetMission()
    {
        var missionsUnitRowId = this.CurrentMissionsUnitRowId;

        if (this.lastMissionUnitRowId != missionsUnitRowId)
        {
            this.lastMissionUnitRowId = missionsUnitRowId;
            this.currentMission = missionsUnitRowId > 0 ? this.missionUnitSheet.GetRow(this.lastMissionUnitRowId).Name.ExtractText() : string.Empty;
            this.selectedRecipe = null;
            this.selectedRotationResolver = null;
            this.nextCraftDataInfo = null;
        }
    }

    private sealed record CraftDataInfo(Recipe Recipe, uint ItemId, uint Required, (uint ItemId, byte Amount)[] Ingredient, bool RequireHq);
}