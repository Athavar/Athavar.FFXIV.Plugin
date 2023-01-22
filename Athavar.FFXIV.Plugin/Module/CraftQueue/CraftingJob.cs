// <copyright file="CraftingJob.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.CraftQueue;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Athavar.FFXIV.Plugin.Extension;
using Athavar.FFXIV.Plugin.Lib.ClickLib.Clicks;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;
using Athavar.FFXIV.Plugin.Utils.Constants;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;
using Recipe = Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Recipe;

internal class CraftingJob
{
    private readonly string localizedExcellent;
    private readonly Func<int>[] stepArray;

    private readonly CraftQueue queue;
    private readonly Simulation simulation;

    private readonly CraftingSkills[] rotation;

    private readonly BuffInfo? food;
    private readonly BuffInfo? potion;

    private readonly int hqPercent;
    private readonly bool trial;
    private TimeSpan lastLoopDuration = TimeSpan.Zero;

    private SimulationResult simulationResult;

    private int waitMs;

    public CraftingJob(CraftQueue queue, Recipe recipe, RotationNode node, CrafterStats crafterStats, uint count, BuffInfo? food, BuffInfo? potion, (uint ItemId, byte Amount)[] hqIngredients)
    {
        this.queue = queue;
        this.Recipe = recipe;
        this.Loops = count;

        this.food = food;
        this.potion = potion;

        this.localizedExcellent = this.queue.DalamudServices.DataManager.GetExcelSheet<Addon>()?.GetRow(228)?.Text.ToString().ToLowerInvariant() ?? throw new AthavarPluginException();

        this.rotation = node.Rotations;
        this.HqIngredients = hqIngredients;

        this.simulation = new Simulation(crafterStats, this.Recipe)
        {
            CurrentStatModifiers = new[] { food?.Stats, potion?.Stats },
        };
        this.simulation.SetHqIngredients(hqIngredients);
        this.RunSimulation();
        this.hqPercent = this.simulationResult.HqPercent;

        this.stepArray = new[]
        {
            this.SwitchToJob,
            this.EnsureRepair,
            this.EnsureStats,
            this.OpenRecipe,
            this.SelectIngredients,
            this.StartCraft,
            this.WaitSynthesis,
            this.DoRotationAction,
        };
    }

    [Flags]
    private enum BuffApplyTest
    {
        None = 0,
        Food = 1,
        Potion = 2,
        Both = Food | Potion,
    }

    internal uint Loops { get; }

    internal uint RemainingLoops => this.Loops - this.CurrentLoop;

    internal Recipe Recipe { get; }

    internal (uint ItemId, byte Amount)[] HqIngredients { get; }

    internal int RotationMaxSteps => this.rotation.Length;

    internal TimeSpan Duration => this.DurationWatch.Elapsed;

    internal TimeSpan LoopDuration => this.CurrentLoop == 0 ? this.Duration : this.lastLoopDuration / this.CurrentLoop;

    internal uint CurrentLoop { get; private set; }

    internal int CurrentStep { get; private set; }

    internal int RotationCurrentStep { get; private set; }

    internal JobStatus Status { get; private set; }

    private Stopwatch Stopwatch { get; } = new();

    private Stopwatch DurationWatch { get; } = new();

    private IList<ActionResult> Steps => this.simulationResult.Steps;

    private bool IsKneeling => this.queue.DalamudServices.Condition[ConditionFlag.PreparingToCraft];

    internal void Pause()
    {
        this.Status = JobStatus.Paused;
        if (this.DurationWatch.IsRunning)
        {
            this.DurationWatch.Stop();
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

    [MemberNotNull(nameof(simulationResult))]
    private void RunSimulation() => this.simulationResult = this.simulation.Run(this.rotation, true);

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

    private int SwitchToJob()
    {
        var ci = this.queue.CommandInterface;
        var playerJob = (Job?)this.queue.DalamudServices.ClientState.LocalPlayer?.ClassJob.Id ?? Job.Adventurer;
        var recipeJob = this.Recipe.Class.GetJob();
        if (playerJob == recipeJob)
        {
            var current = this.queue.GearsetManager.GetCurrentEquipment() ?? throw new CraftingJobException("Fail to get the current gear-stats.");
            this.simulation.CrafterStats = current.ToCrafterStats();
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

        var gs = gearsetManager.AllGearsets.FirstOrDefault(g => g.JobClass == recipeJob);
        if (gs is null)
        {
            throw new CraftingJobException("Could not find a gearset with the required job.");
        }

        gearsetManager.EquipGearset(gs.Id);
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

        // is in crafting mode
        if (this.IsKneeling)
        {
            if (ci.IsAddonVisible(Constants.Addons.RecipeNote))
            {
                ci.CloseAddon(Constants.Addons.RecipeNote);
                return -1000;
            }
        }

        if (ci.IsAddonVisible(Constants.Addons.RecipeNote))
        {
            ci.CloseAddon(Constants.Addons.RecipeNote);
            return -100;
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
            return -100;
        }

        // open repair window
        if (!ci.UseGeneralAction(6))
        {
            throw new CraftingJobException("Fail to open repair window");
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

        var buffApplyStats = this.TestStatModifier();

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

        if (this.food is not null && (buffApplyStats & BuffApplyTest.Food) != 0)
        {
            var itemId = this.food.Item.RowId;
            var count = ci.CountItem(this.food.Item.RowId, this.food.IsHq);
            if (count == 0)
            {
                throw new CraftingJobException("Missing food in inventory.");
            }

            ci.UseItem(itemId, this.food.IsHq);
            return -1000;
        }

        if (this.potion is not null && (buffApplyStats & BuffApplyTest.Potion) != 0)
        {
            var itemId = this.potion.Item.RowId;
            var count = ci.CountItem(itemId, this.potion.IsHq);
            if (count == 0)
            {
                throw new CraftingJobException("Missing potion in inventory.");
            }

            if (!ci.UseItem(itemId, this.potion.IsHq))
            {
                throw new CraftingJobException("Fail to use potion. Is perhaps on CD.");
            }

            return -1000;
        }

        return 0;
    }

    private int OpenRecipe()
    {
        var ci = this.queue.CommandInterface;
        var recipeId = this.Recipe.GameRecipe.RowId;

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

            return 0;
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

        var buffApplyStats = this.TestStatModifier();

        if (buffApplyStats is null || buffApplyStats != BuffApplyTest.None)
        {
            this.CurrentStep = 0;
            return -1000;
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

        if (this.RotationCurrentStep >= this.rotation.Length || !ci.IsAddonVisible("Synthesis"))
        {
            if (ci.IsAddonVisible(Constants.Addons.Synthesis))
            {
                return -100;
            }

            ++this.CurrentLoop;
            this.RotationCurrentStep = 0;
            this.CurrentStep = 0;
            this.lastLoopDuration = this.DurationWatch.Elapsed;
        }

        if (this.CurrentLoop >= this.Loops)
        {
            return 0;
        }

        var action = this.Steps[this.RotationCurrentStep];
        if (this.RotationCurrentStep + 1 < this.rotation.Length &&
            this.rotation[this.RotationCurrentStep + 1] == CraftingSkills.ByregotsBlessing &&
            ci.HasCondition(this.localizedExcellent) &&
            ci.UseAction(CraftingSkill.FindAction(CraftingSkills.TricksOfTheTrade).Action.GetId(this.Recipe.Class)))
        {
            return -1000;
        }


        if (!ci.UseAction(action.Skill.Action.GetId(this.Recipe.Class)))
        {
            if (action.FailCause == SimulationFailCause.NOT_ENOUGH_CP || action.Skill.Action.SkipOnFail())
            {
                ++this.RotationCurrentStep;
            }

            return -10;
        }

        ++this.RotationCurrentStep;
        return -1000;
    }

    private BuffApplyTest? TestStatModifier()
    {
        var currentStatModifier = this.CurrentStatModifier();
        this.simulation.CurrentStatModifiers = (StatModifiers?[])currentStatModifier.Clone();

        var buffApplyStats = BuffApplyTest.None;
        for (var i = 0; i < 4; i++)
        {
            buffApplyStats = (BuffApplyTest)i;
            if (this.food is not null && (buffApplyStats & BuffApplyTest.Food) != 0)
            {
                this.simulation.CurrentStatModifiers[0] = this.food.Stats;
            }

            if (this.potion is not null && (buffApplyStats & BuffApplyTest.Potion) != 0)
            {
                this.simulation.CurrentStatModifiers[1] = this.potion.Stats;
            }

            this.RunSimulation();

            if (this.food is not null && (buffApplyStats & BuffApplyTest.Food) != 0)
            {
                this.simulation.CurrentStatModifiers[0] = currentStatModifier[0];
            }

            if (this.potion is not null && (buffApplyStats & BuffApplyTest.Potion) != 0)
            {
                this.simulation.CurrentStatModifiers[1] = currentStatModifier[1];
            }

            if (this.simulationResult.Success && this.simulationResult.HqPercent >= this.hqPercent)
            {
                break;
            }

            if (buffApplyStats == BuffApplyTest.Both)
            {
                return null;
            }
        }

        return buffApplyStats;
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