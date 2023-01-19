// <copyright file="CraftingSkills.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Extension;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Buff;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Other;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Progression;
using Athavar.FFXIV.Plugin.Lib.CraftSimulation.Models.Actions.Quality;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;
using Microsoft.VisualBasic.CompilerServices;
using Action = Lumina.Excel.GeneratedSheets.Action;

internal record CraftingSkill(CraftingSkills Skill, CraftingAction Action, MultiString Name, ushort IconId)
{
    private static readonly Regex ActionRegex = new(@"^(/(?:ac|action)\s+)?(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex WaitRegex = new(@"(?<modifier><wait\.(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static Dictionary<CraftingSkills, CraftingSkill>? actions;

    public static void Populate(DataManager dataManager)
        => actions = new CraftingSkillCollectionBuilder(dataManager)
            /* Class Actions  */
           .Add(CraftingSkills.BasicSynthesis, new BasicSynthesis())
           .Add(CraftingSkills.BasicTouch, new BasicTouch())
           .Add(CraftingSkills.MastersMend, new MastersMend())
           .Add(CraftingSkills.HastyTouch, new HastyTouch())
           .Add(CraftingSkills.RapidSynthesis, new RapidSynthesis())
           .Add(CraftingSkills.Observe, new Observe())
           .Add(CraftingSkills.TricksOfTheTrade, new TricksOfTheTrade())
           .Add(CraftingSkills.WasteNot, new WasteNot())
           .Add(CraftingSkills.Veneration, new Veneration())
           .Add(CraftingSkills.StandardTouch, new StandardTouch())
           .Add(CraftingSkills.GreatStrides, new GreatStrides())
           .Add(CraftingSkills.Innovation, new Innovation())
           .Add(CraftingSkills.FinalAppraisal, new FinalAppraisal())
           .Add(CraftingSkills.WasteNotII, new WasteNotII())
           .Add(CraftingSkills.ByregotsBlessing, new ByregotsBlessing())
           .Add(CraftingSkills.PreciseTouch, new PreciseTouch())
           .Add(CraftingSkills.MuscleMemory, new MuscleMemory())
           .Add(CraftingSkills.CarefulSynthesis, new CarefulSynthesis())
           .Add(CraftingSkills.Manipulation, new Manipulation())
           .Add(CraftingSkills.PrudentTouch, new PrudentTouch())
           .Add(CraftingSkills.FocusedSynthesis, new FocusedSynthesis())
           .Add(CraftingSkills.Reflect, new Reflect())
           .Add(CraftingSkills.PreparatoryTouch, new PreparatoryTouch())
           .Add(CraftingSkills.Groundwork, new Groundwork())
           .Add(CraftingSkills.DelicateSynthesis, new DelicateSynthesis())
           .Add(CraftingSkills.IntensiveSynthesis, new IntensiveSynthesis())
           .Add(CraftingSkills.TrainedEye, new TrainedEye())
           .Add(CraftingSkills.AdvancedTouch, new AdvancedTouch())
           .Add(CraftingSkills.PrudentSynthesis, new PrudentSynthesis())
           .Add(CraftingSkills.TrainedFinesse, new TrainedFinesse())
            /* Specialist Actions  */
           .Add(CraftingSkills.CarefulObservation, new CarefulObservation())
           .Add(CraftingSkills.HearthAndSoul, new HeartAndSoul())
           .Build();

    public static CraftingSkill FindAction(CraftingSkills skill) => actions?[skill] ?? throw new AthavarPluginException();

    public static CraftingSkill? FindAction(string name) => actions?.Values.FirstOrDefault(a => a.Name.Equals(name));

    public static IReadOnlyCollection<CraftingSkill> AllSkills() => actions?.Values ?? throw new IncompleteInitialization();

    public static CraftingMacro Parse(CraftingSkills[] skills)
        => new()
        {
            Rotation = skills.Select(s => actions![s]).ToArray(),
        };

    public static CraftingSkills[] Parse(string text)
    {
        string ExtractAndUnquote(Match match, string groupName)
        {
            var group = match.Groups[groupName];
            var groupValue = group.Value;

            if (groupValue.StartsWith('"') && groupValue.EndsWith('"'))
            {
                groupValue = groupValue.Trim('"');
            }

            return groupValue;
        }

        string? line;
        using var reader = new StringReader(text);

        List<CraftingSkills> skillsList = new();
        while ((line = reader.ReadLine()) != null)
        {
            var match = WaitRegex.Match(line);

            if (match.Success)
            {
                var group = match.Groups["modifier"];
                line = line.Remove(group.Index, group.Length);
            }

            match = ActionRegex.Match(line);
            if (match.Success)
            {
                var name = match.ExtractAndUnquote("name");
                var skill = FindAction(name);
                if (skill is not null)
                {
                    skillsList.Add(skill.Skill);
                }
            }
        }

        return skillsList.ToArray();
    }

    private class CraftingSkillCollectionBuilder
    {
        private readonly DataManager dataManager;
        private Dictionary<CraftingSkills, CraftingSkill>? dictionary = new();

        public CraftingSkillCollectionBuilder(DataManager dataManager) => this.dataManager = dataManager;

        public CraftingSkillCollectionBuilder Add(CraftingSkills key, CraftingAction value)
        {
            if (this.dictionary == null)
            {
                throw new InvalidOperationException();
            }


            var actionId = value.GetId(value.Job);
            var useCraftActionSheet = actionId > 100000;

            ushort iconId = 0;

            if (useCraftActionSheet)
            {
                var row = this.dataManager.GetExcelSheet<CraftAction>()!.GetRow(actionId);
                if (actionId != 0 && row is not null)
                {
                    iconId = row.Icon;
                }
            }
            else
            {
                var row = this.dataManager.GetExcelSheet<Action>()!.GetRow(actionId);
                if (actionId != 0 && row is not null)
                {
                    iconId = row.Icon;
                }
            }

            this.dictionary.Add(
                key,
                new CraftingSkill(
                    key,
                    value,
                    useCraftActionSheet ? MultiString.FromCraftAction(this.dataManager, actionId) : MultiString.FromAction(this.dataManager, actionId),
                    iconId));
            return this;
        }

        public Dictionary<CraftingSkills, CraftingSkill> Build()
        {
            var @ref = this.dictionary!;
            this.dictionary = null;
            return @ref;
        }
    }
}

public enum CraftingSkills
{
    BasicSynthesis = 1,
    BasicTouch,
    MastersMend,
    HastyTouch,
    RapidSynthesis,
    Observe,
    TricksOfTheTrade,
    WasteNot,
    Veneration,
    StandardTouch,
    GreatStrides,
    Innovation,
    FinalAppraisal,
    WasteNotII,
    ByregotsBlessing,
    PreciseTouch,
    MuscleMemory,
    CarefulSynthesis,
    Manipulation,
    PrudentTouch,
    FocusedSynthesis,
    Reflect,
    PreparatoryTouch,
    Groundwork,
    DelicateSynthesis,
    IntensiveSynthesis,
    TrainedEye,
    AdvancedTouch,
    PrudentSynthesis,
    TrainedFinesse,
    CarefulObservation = 100,
    HearthAndSoul,
}