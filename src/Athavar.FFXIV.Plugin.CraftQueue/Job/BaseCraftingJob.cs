// <copyright file="BaseCraftingJob.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.CraftQueue.Job;

using System.Diagnostics;
using Athavar.FFXIV.Plugin.Click.Clicks;
using Athavar.FFXIV.Plugin.Common.Exceptions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.CraftQueue.Resolver;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;
using Athavar.FFXIV.Plugin.Models;
using Dalamud.Game.ClientState.Conditions;

internal abstract class BaseCraftingJob
{
    protected readonly CraftQueue queue;
    private readonly Func<int>[] stepArray;
    private readonly int stepCraftStartIndex;

    private readonly Gearset gearset;

    private readonly IRotationResolver rotationResolver;

    private readonly CraftingJobFlags flags;
    private readonly bool trial;

    private TimeSpan lastLoopDuration = TimeSpan.Zero;
    protected int currentRotationStep;

    private int waitMs;

    public BaseCraftingJob(CraftQueue queue, RecipeExtended recipe, IRotationResolver rotationResolver, Gearset gearset, uint count, BuffConfig buffConfig, (uint ItemId, byte Amount)[] hqIngredients, CraftingJobFlags flags)
    {
        this.queue = queue;
        this.Recipe = recipe;
        this.Loops = count;
        this.gearset = gearset;

        this.BuffConfig = buffConfig;
        this.flags = flags;

        this.rotationResolver = rotationResolver;

        if (rotationResolver.ResolverType == ResolverType.Dynamic)
        {
            // resolver is dynamic. enforce buffs
            this.flags |= CraftingJobFlags.ForceFood | CraftingJobFlags.ForcePotion;
        }

        // clone hqIngredients
        this.HqIngredients = hqIngredients.Select(i => (i.ItemId, i.Amount)).ToArray();

        this.stepArray =
        [
            this.EnsureInventorySpace,
            this.SwitchToGearset,
            this.EnsureRepair,
            this.EnsureMateriaExtracted,
            this.EnsureStats,
            this.OpenRecipe,
            this.SelectIngredients,
            this.StartCraft,
            this.WaitSynthesis,
            this.DoRotationAction,
        ];

        this.stepCraftStartIndex = Array.IndexOf(this.stepArray, this.StartCraft);
    }

    [Flags]
    protected enum BuffApplyTest
    {
        None = 0,
        Food = 1,
        Potion = 2,
        Both = Food | Potion,
    }

    public BuffConfig BuffConfig { get; }

    public string RotationName => this.rotationResolver.Name;

    internal uint Loops { get; }

    internal uint RemainingLoops => this.Loops - this.CurrentLoop;

    internal RecipeExtended Recipe { get; }

    internal (uint ItemId, byte Amount)[] HqIngredients { get; }

    internal int RotationMaxSteps => this.rotationResolver.Length;

    internal TimeSpan Duration => this.DurationWatch.Elapsed;

    internal TimeSpan LoopDuration => this.CurrentLoop == 0 ? this.Duration : this.lastLoopDuration / this.CurrentLoop;

    internal uint CurrentLoop { get; private set; }

    internal int CurrentStep { get; private set; }

    internal JobStatus Status { get; private set; }

    internal int RotationCurrentStep => this.currentRotationStep;

    private Stopwatch Stopwatch { get; } = new();

    private Stopwatch DurationWatch { get; } = new();

    private bool IsKneeling => this.queue.DalamudServices.Condition[ConditionFlag.PreparingToCraft];

    internal void Pause()
    {
        if (this.Status == JobStatus.Paused)
        {
            return;
        }

        this.Status = JobStatus.Paused;
        if (this.DurationWatch.IsRunning)
        {
            this.DurationWatch.Stop();
        }

        if (this.CurrentStep <= this.stepCraftStartIndex)
        {
            this.CurrentStep = 0;
        }
    }

    internal void Cancel()
    {
        this.Failure();
        this.Status = JobStatus.Canceled;
    }

    internal void Failure()
    {
        this.Status = JobStatus.Failure;
        this.DurationWatch.Stop();
        this.Stopwatch.Stop();
    }

    internal bool Tick()
    {
        if (this.Status is not (JobStatus.Paused or JobStatus.Queued or JobStatus.Running))
        {
            return true;
        }

        if (!this.DurationWatch.IsRunning)
        {
            this.DurationWatch.Start();
            this.Status = JobStatus.Running;
        }

        if (this.CurrentLoop >= this.Loops)
        {
            this.Status = JobStatus.Success;
            this.DurationWatch.Stop();
            return true;
        }

        if (this.Stopwatch.Elapsed < TimeSpan.FromMilliseconds(this.waitMs))
        {
            return false;
        }

        this.InternalTick();
        return false;
    }

    protected virtual void OnCheckCurrentEquipment(Gearset currentEquipped)
    {
    }

    protected virtual BuffApplyTest? CalcMissingBuffs(StatModifiers?[] currentStatModifier) => BuffApplyTest.None;

    protected virtual int? MutateRotation() => null;

    protected virtual void ActionUseFailed(CraftingSkill skill)
    {
    }

    private void InternalTick()
    {
        this.Stopwatch.Restart();

        var num = this.stepArray[this.CurrentStep]();
        if (num >= 0)
        {
            ++this.CurrentStep;
        }

        this.waitMs = Math.Abs(num);
    }

    private int EnsureInventorySpace()
    {
        if (this.queue.CommandInterface.FreeInventorySlots() <= 0)
        {
            this.queue.Pause();
            return -1000;
        }

        return 0;
    }

    private int SwitchToGearset()
    {
        var current = this.queue.GearsetManager.GetCurrentEquipment() ?? throw new CraftingJobException("Fail to get the current gear-stats.");
        var ci = this.queue.CommandInterface;

        this.queue.DalamudServices.PluginLogger.Information(" selected:{0} current:{1}", this.gearset.Id, current.Id);
        if (this.gearset.Id == current.Id)
        {
            this.OnCheckCurrentEquipment(current);
            return 0;
        }

        // is in crafting mode
        if (this.IsKneeling)
        {
            if (ci.IsAddonVisible(Constants.Addons.RecipeNote))
            {
                ci.CloseAddon(Constants.Addons.RecipeNote);
            }

            return -1000;
        }

        var gearsetManager = this.queue.GearsetManager;

        gearsetManager.EquipGearset(this.gearset.Id);
        return -500;
    }

    private int EnsureRepair()
    {
        var ci = this.queue.CommandInterface;

        // check for repair
        if (!ci.NeedsRepair())
        {
            if (ci.IsAddonVisible(Constants.Addons.Repair))
            {
                ci.CloseAddon(Constants.Addons.Repair);
                return -1000;
            }

            return 0;
        }

        if (!this.queue.Configuration.AutoRepair)
        {
            throw new CraftingJobException("Gear is not repaired.");
        }

        var value = this.ExitCraftingMode();
        if (value is { } wait)
        {
            return wait;
        }

        // is currently repairing
        if (this.queue.DalamudServices.Condition[ConditionFlag.Occupied39])
        {
            return -100;
        }

        // say yes to repair
        if (ci.IsAddonVisible("SelectYesno"))
        {
            this.queue.Click.TrySendClick("select_yes");
            return -100;
        }

        // repair all
        if (ci.IsAddonVisible(Constants.Addons.Repair))
        {
            this.queue.Click.TrySendClick("repair_all");
            return -1000;
        }

        // open repair window
        if (!ci.UseGeneralAction(6))
        {
            throw new CraftingJobException("Fail to open repair window");
        }

        return -100;
    }

    private int EnsureMateriaExtracted()
    {
        var ci = this.queue.CommandInterface;

        if (!this.queue.Configuration.AutoMateriaExtract)
        {
            // skip step
            return 0;
        }

        // can extract materia?
        if (!ci.CanExtractMateria())
        {
            if (ci.IsAddonVisible(Constants.Addons.Materialize))
            {
                ci.CloseAddon(Constants.Addons.Materialize);
                return -1000;
            }

            return 0;
        }

        var value = this.ExitCraftingMode();
        if (value is { } wait)
        {
            return wait;
        }

        // is currently extracting
        if (this.queue.DalamudServices.Condition[ConditionFlag.Occupied39])
        {
            return -100;
        }

        // say yes to extract
        if (ci.IsAddonVisible("MaterializeDialog"))
        {
            this.queue.Click.TrySendClick("materialize");
            return -100;
        }

        // materia extract item0
        if (ci.IsAddonVisible(Constants.Addons.Materialize))
        {
            this.queue.Click.TrySendClick("materia_extract0");
            return -1000;
        }

        // open materialize window
        if (!ci.UseGeneralAction(14))
        {
            throw new CraftingJobException("Fail to open materialize window");
        }

        return -100;
    }

    private int EnsureStats()
    {
        var localPlayer = this.queue.DalamudServices.ClientState.LocalPlayer;
        if (localPlayer == null)
        {
            return -1000;
        }

        var buffApplyStats = this.GetMissingBuffs();

        if (buffApplyStats is null)
        {
            throw new CraftingJobException("Something is wrong with your stats. please recheck the gearsets.");
        }

        if (buffApplyStats == BuffApplyTest.None)
        {
            return 0;
        }

        var ci = this.queue.CommandInterface;

        if (this.IsKneeling)
        {
            if (ci.IsAddonVisible(Constants.Addons.RecipeNote))
            {
                ci.CloseAddon(Constants.Addons.RecipeNote);
                return -1000;
            }
        }

        if (this.BuffConfig.Food is { } food && (buffApplyStats & BuffApplyTest.Food) != 0)
        {
            var itemId = food.ItemId;
            var count = ci.CountItem(itemId, food.IsHq);
            if (count == 0)
            {
                throw new CraftingJobException("Missing food in inventory.");
            }

            ci.UseItem(itemId, food.IsHq);
            return -1000;
        }

        if (this.BuffConfig.Potion is { } potion && (buffApplyStats & BuffApplyTest.Potion) != 0)
        {
            var itemId = potion.ItemId;
            var count = ci.CountItem(itemId, potion.IsHq);
            if (count == 0)
            {
                throw new CraftingJobException("Missing potion in inventory.");
            }

            ci.UseItem(itemId, potion.IsHq);
            return -1000;
        }

        return 0;
    }

    private int OpenRecipe()
    {
        var ci = this.queue.CommandInterface;
        var recipeId = this.Recipe.RecipeId;

        var selectedRecipeItemId = ci.GetRecipeNoteSelectedRecipeId();
        if ((!ci.IsAddonVisible(Constants.Addons.RecipeNote) && !ci.IsAddonVisible(Constants.Addons.Synthesis)) || (ci.IsAddonVisible(Constants.Addons.RecipeNote) && (selectedRecipeItemId == -1 || recipeId != selectedRecipeItemId)))
        {
            ci.OpenRecipeByRecipeId(recipeId);
            return -500;
        }

        return !ci.IsAddonVisible(Constants.Addons.RecipeNote) ? -1000 : 100;
    }

    private int SelectIngredients()
    {
        var ci = this.queue.CommandInterface;
        if (!ci.IsAddonVisible(Constants.Addons.RecipeNote))
        {
            return -100;
        }

        var ptr = this.queue.DalamudServices.GameGui.GetAddonByName(Constants.Addons.RecipeNote);
        if (ptr == nint.Zero)
        {
            return -100;
        }

        var click = ClickRecipeNote.Using(ptr);

        click.MaterialNq();
        if (this.HqIngredients.Length != 0)
        {
            for (var idx = 0; idx < this.Recipe.Ingredients.Length; ++idx)
            {
                var ingredient = this.Recipe.Ingredients[idx];
                var found = this.HqIngredients.FirstOrDefault(i => i.ItemId == ingredient.Id);
                if (found != default)
                {
                    for (var index = 0; index < found.Amount; ++index)
                    {
                        click.Material(idx, true);
                    }
                }
            }
        }

        return 0;
    }

    private int StartCraft()
    {
        if (!this.queue.CommandInterface.IsAddonVisible(Constants.Addons.RecipeNote))
        {
            return -100;
        }

        this.queue.Click.TrySendClick(this.trial ? "trial_synthesis" : "synthesize");

        return 0;
    }

    private int WaitSynthesis()
    {
        if (!this.queue.CommandInterface.IsAddonVisible(Constants.Addons.Synthesis))
        {
            return -100;
        }

        var buffApplyStats = this.GetMissingBuffs();

        if (buffApplyStats is null || buffApplyStats != BuffApplyTest.None)
        {
            var ci = this.queue.CommandInterface;

            ci.CloseAddon(Constants.Addons.Synthesis);
            this.CurrentStep = 0;
            return -1500;
        }

        return 0;
    }

    private int DoRotationAction()
    {
        if (this.queue.DalamudServices.Condition[ConditionFlag.Crafting40])
        {
            return -25;
        }

        var ci = this.queue.CommandInterface;

        if (this.RotationCurrentStep >= this.rotationResolver.Length || !ci.IsAddonVisible(Constants.Addons.Synthesis))
        {
            if (ci.IsAddonVisible(Constants.Addons.Synthesis))
            {
                // wait exiting of craft.
                return -100;
            }

            // finish craft of item
            ++this.CurrentLoop;
            this.currentRotationStep = 0;
            this.CurrentStep = 0;
            this.lastLoopDuration = this.DurationWatch.Elapsed;
        }

        if (this.CurrentLoop >= this.Loops)
        {
            return 0;
        }

        var c = this.queue.Configuration;

        var nextAction = this.rotationResolver.GetNextAction(this.RotationCurrentStep);
        if (nextAction is null)
        {
            // wait
            return -10;
        }

        var skill = CraftingSkill.FindAction(nextAction.Value);

        // maxQuality check
        try
        {
            if (c.QualitySkip && skill.Action.ActionType is ActionType.Quality
                              && ci.HasMaxQuality())
            {
                ++this.currentRotationStep;
                return -1;
            }
        }
        catch (AthavarPluginException ex)
        {
            this.queue.DalamudServices.PluginLogger.Error(ex, "Error while try to check HasMaxQuality");
            return -100;
        }

        var mutate = this.MutateRotation();
        if (mutate is { } wait)
        {
            return wait;
        }

        var simAction = skill.Action;

        if (!ci.UseAction(simAction.GetId(this.Recipe.Class)))
        {
            this.ActionUseFailed(skill);
            return -10;
        }

        ++this.currentRotationStep;

        if (c.CraftWaitSkip)
        {
            return -1000;
        }

        return -1000 * simAction.GetWaitDuration();
    }

    private int? ExitCraftingMode()
    {
        var ci = this.queue.CommandInterface;

        // is in crafting mode
        if (this.IsKneeling)
        {
            if (ci.IsAddonVisible(Constants.Addons.RecipeNote))
            {
                ci.CloseAddon(Constants.Addons.RecipeNote);
                return -1000;
            }
        }

        if (!ci.IsPlayerCharacterReady())
        {
            return -100;
        }

        if (ci.IsAddonVisible(Constants.Addons.RecipeNote))
        {
            ci.CloseAddon(Constants.Addons.RecipeNote);
            return -100;
        }

        return null;
    }

    private BuffApplyTest? GetMissingBuffs()
    {
        var currentStatModifier = this.CurrentStatModifier();

        // check forced food apply
        if (this.BuffConfig.Food is not null && this.flags.HasFlagFast(CraftingJobFlags.ForceFood) && currentStatModifier[0] == null)
        {
            return BuffApplyTest.Food;
        }

        // check forced potion apply
        if (this.BuffConfig.Potion is not null && this.flags.HasFlagFast(CraftingJobFlags.ForcePotion) && currentStatModifier[1] == null)
        {
            return BuffApplyTest.Food;
        }

        return this.CalcMissingBuffs(currentStatModifier);
    }

    private StatModifiers?[] CurrentStatModifier()
    {
        var player = this.queue.DalamudServices.ClientState.LocalPlayer;
        if (player is null)
        {
            return new StatModifiers?[2];
        }

        var modifier = new StatModifiers?[2];
        foreach (var status in player.StatusList)
        {
            switch (status.StatusId)
            {
                case 48:
                case 49:
                    var row = status.Param;
                    var isHq = row >= 10000;
                    if (isHq)
                    {
                        row -= 10000;
                    }

                    if (status.StatusId == 48U)
                    {
                        var currentFood = this.queue.Data.Foods.FirstOrDefault(f => f.ItemFoodId == row && f.IsHq == isHq);
                        if (currentFood is not null)
                        {
                            modifier[0] = currentFood.Stats;
                        }
                    }

                    if (status.StatusId == 49U)
                    {
                        var currentPotion = this.queue.Data.Potions.FirstOrDefault(f => f.ItemFoodId == row && f.IsHq == isHq);
                        if (currentPotion is not null)
                        {
                            modifier[1] = currentPotion.Stats;
                        }
                    }

                    continue;
                default:
                    continue;
            }
        }

        return modifier;
    }
}