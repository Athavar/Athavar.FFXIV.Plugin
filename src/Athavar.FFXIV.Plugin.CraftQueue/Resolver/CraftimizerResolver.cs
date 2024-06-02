namespace Athavar.FFXIV.Plugin.CraftQueue.Resolver;

using System.Collections;
using System.Reflection;
using Athavar.FFXIV.Plugin.CraftSimulator.Models;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Interface.Windowing;

internal class CraftimizerResolver(IPluginManagerWrapper pluginManagerWrapper, IPluginLogger pluginLogger) : IRotationResolver
{
    private bool init;
    private SynthHelpWrapper? synthHelpWrapper;
    private int errorCount;
    private int waitTick;

    public ResolverType ResolverType => ResolverType.Dynamic;

    public string Name => "[Dynamic] Craftimizer";

    public int Length => -1;

    public CraftingSkills? GetNextAction(int index)
    {
        this.Init();
        if (this.synthHelpWrapper is null)
        {
            this.waitTick = 0;
            return null;
        }

        if (this.synthHelpWrapper.IsSolving())
        {
            // is currently calculating.
            this.waitTick = 0;
            return null;
        }

        var macro = this.synthHelpWrapper.GetMacro();

        if (macro is null)
        {
            this.waitTick = 0;
            return null;
        }

        var skill = macro.GetFirstCraftingSkill(pluginLogger);
        if (skill is null)
        {
            this.errorCount++;

            // approx. 1 seconds
            if (this.errorCount > 100)
            {
                this.synthHelpWrapper.Retry();
            }
        }
        else
        {
            this.errorCount = 0;
            this.waitTick++;
            if (this.waitTick < 3)
            {
                // wait. Perhaps a recalculation will be triggered because of state change.
                return null;
            }

            this.waitTick = 0;
        }

        return skill;
    }

    private static CraftingSkills ToCraftingSkill(byte value)
        => value switch
        {
            0 => CraftingSkills.AdvancedTouch,
            1 => CraftingSkills.BasicSynthesis,
            2 => CraftingSkills.BasicTouch,
            3 => CraftingSkills.ByregotsBlessing,
            4 => CraftingSkills.CarefulObservation,
            5 => CraftingSkills.CarefulSynthesis,
            6 => CraftingSkills.DelicateSynthesis,
            7 => CraftingSkills.FinalAppraisal,
            8 => CraftingSkills.FocusedSynthesis,
            9 => CraftingSkills.FocusedTouch,
            10 => CraftingSkills.GreatStrides,
            11 => CraftingSkills.Groundwork,
            12 => CraftingSkills.HastyTouch,
            13 => CraftingSkills.HearthAndSoul,
            14 => CraftingSkills.Innovation,
            15 => CraftingSkills.IntensiveSynthesis,
            16 => CraftingSkills.Manipulation,
            17 => CraftingSkills.MastersMend,
            18 => CraftingSkills.MuscleMemory,
            19 => CraftingSkills.Observe,
            20 => CraftingSkills.PreciseTouch,
            21 => CraftingSkills.PreparatoryTouch,
            22 => CraftingSkills.PrudentSynthesis,
            23 => CraftingSkills.PrudentTouch,
            24 => CraftingSkills.RapidSynthesis,
            25 => CraftingSkills.Reflect,
            26 => CraftingSkills.StandardTouch,
            27 => CraftingSkills.TrainedEye,
            28 => CraftingSkills.TrainedFinesse,
            29 => CraftingSkills.TricksOfTheTrade,
            30 => CraftingSkills.Veneration,
            31 => CraftingSkills.WasteNot,
            32 => CraftingSkills.WasteNotII,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
        };

    private void Init()
    {
        if (this.init)
        {
            return;
        }

        var instance = pluginManagerWrapper.GetPluginInstance("Craftimizer");

        var synthHelpWindow = instance?.GetType().GetProperty("SynthHelperWindow", BindingFlags.Instance | BindingFlags.Public)?.GetValue(instance);
        if (synthHelpWindow is Window window)
        {
            this.synthHelpWrapper = new SynthHelpWrapper(window);
        }

        this.init = true;
    }

    private class SynthHelpWrapper(Window window)
    {
        public SimulatedMacroWrapper? GetMacro()
        {
            var value = window.GetType().GetProperty("Macro", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window);
            if (value is null)
            {
                return null;
            }

            return new SimulatedMacroWrapper(value);
        }

        public bool IsSolving()
        {
            var value = window.GetType().GetProperty("HelperTaskRunning", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window);
            if (value is bool b)
            {
                return b;
            }

            return true;
        }

        public void Retry()
        {
            var method = window.GetType().GetMethod("CalculateBestMacro", BindingFlags.Instance | BindingFlags.NonPublic);
            method?.Invoke(window, null);
        }
    }

    private class SimulatedMacroWrapper(object o)
    {
        public CraftingSkills? GetFirstCraftingSkill(IPluginLogger logger)
        {
            var listProperty = o.GetType().GetProperty("Macro", BindingFlags.Instance | BindingFlags.NonPublic);
            if (listProperty is null)
            {
                return null;
            }

            if (listProperty.GetValue(o) is not IList list || list.Count < 1)
            {
                return null;
            }

            var stepType = listProperty.PropertyType.GetGenericArguments()[0];
            var value = stepType.GetProperty("Action", BindingFlags.Instance | BindingFlags.Public)?.GetValue(list[0]);
            if (value is null)
            {
                return null;
            }

            var enumValue = Convert.ToByte(value);

            return ToCraftingSkill(enumValue);
        }
    }
}