// <copyright file="CraftingSkill.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.CraftSimulator.Models;

using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Buff;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Deprecated;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Other;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Progression;
using Athavar.FFXIV.Plugin.CraftSimulator.Models.Actions.Quality;
using Microsoft.VisualBasic.CompilerServices;

public sealed record CraftingSkill(CraftingSkills Skill, CraftingAction Action)
{
    private static readonly Dictionary<CraftingSkills, CraftingSkill> Actions = new CraftingSkillCollectionBuilder()
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
           .Add(CraftingSkills.Reflect, new Reflect())
           .Add(CraftingSkills.PreparatoryTouch, new PreparatoryTouch())
           .Add(CraftingSkills.Groundwork, new Groundwork())
           .Add(CraftingSkills.DelicateSynthesis, new DelicateSynthesis())
           .Add(CraftingSkills.IntensiveSynthesis, new IntensiveSynthesis())
           .Add(CraftingSkills.TrainedEye, new TrainedEye())
           .Add(CraftingSkills.AdvancedTouch, new AdvancedTouch())
           .Add(CraftingSkills.PrudentSynthesis, new PrudentSynthesis())
           .Add(CraftingSkills.TrainedFinesse, new TrainedFinesse())
           .Add(CraftingSkills.RefinedTouch, new RefinedTouch())
           .Add(CraftingSkills.DaringTouch, new DaringTouch())
           .Add(CraftingSkills.TrainedPerfection, new TrainedPerfection())
           .Add(CraftingSkills.ImmaculateMend, new ImmaculateMend())
            /* Specialist Actions  */
           .Add(CraftingSkills.CarefulObservation, new CarefulObservation())
           .Add(CraftingSkills.HearthAndSoul, new HeartAndSoul())
           .Add(CraftingSkills.QuickInnovation, new QuickInnovation())
            /* Deprecated Actions */
           .Add(CraftingSkills.FocusedSynthesis, new FocusedSynthesis())
           .Add(CraftingSkills.FocusedTouch, new FocusedTouch())
           .Build();

    public static int[] DeprecatedActionIndex { get; } = Actions.Where(a => a.Value.Action.IsDeprecated).Select(a => (int)a.Key).ToArray();

    public static CraftingSkill FindAction(CraftingSkills skill) => Actions?[skill] ?? throw new IncompleteInitialization();

    public static IReadOnlyCollection<CraftingSkill> AllSkills() => Actions?.Values ?? throw new IncompleteInitialization();

    public static CraftingMacro ParseMacro(IEnumerable<int> skills) => ParseMacro(Parse(skills));

    public static CraftingMacro ParseMacro(IEnumerable<CraftingSkills> skills) => new(skills.Select(s => Actions![s]).ToArray());

    public static IEnumerable<CraftingSkills> Parse(IEnumerable<int> skills) => skills.Cast<CraftingSkills>();

    private sealed class CraftingSkillCollectionBuilder
    {
        private Dictionary<CraftingSkills, CraftingSkill>? dictionary = new();

        public CraftingSkillCollectionBuilder Add(CraftingSkills key, CraftingAction value)
        {
            if (this.dictionary == null)
            {
                throw new InvalidOperationException();
            }

            this.dictionary.Add(
                key,
                new CraftingSkill(
                    key,
                    value));
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