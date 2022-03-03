﻿namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;

using System;
using System.Text.RegularExpressions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     The &lt;condition&gt; modifier.
/// </summary>
internal class ConditionModifier : MacroModifier
{
    private static readonly Regex Regex = new(@"(?<modifier><condition\.(?<not>(not\.|\!))?(?<name>[a-zA-Z]+)>)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string condition;
    private readonly bool negated;

    private ConditionModifier(string condition, bool negated) => (this.condition, this.negated) = (condition, negated);

    /// <summary>
    ///     Parse the text as a modifier.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <param name="command">A parsed modifier.</param>
    /// <returns>A value indicating whether the modifier matched.</returns>
    public static bool TryParse(ref string text, out ConditionModifier command)
    {
        var match = Regex.Match(text);
        var success = match.Success;

        if (success)
        {
            var group = match.Groups["modifier"];
            text = text.Remove(group.Index, group.Length);

            var conditionName = match.Groups["name"].Value.ToLowerInvariant();
            var negated = match.Groups["not"].Success;

            command = new ConditionModifier(conditionName, negated);
        }
        else
        {
            command = new ConditionModifier(string.Empty, false);
        }

        return success;
    }

    /// <summary>
    ///     Check if the current crafting condition is active.
    /// </summary>
    /// <returns>A parsed command.</returns>
    public unsafe bool HasCondition()
    {
        if (this.condition == string.Empty)
        {
            return true;
        }

        var addon = DalamudServices.GameGui.GetAddonByName("Synthesis", 1);
        if (addon == IntPtr.Zero)
        {
            PluginLog.Debug("Could not find Synthesis addon");
            return true;
        }

        var addonPtr = (AddonSynthesis*)addon;
        var text = addonPtr->Condition->NodeText.ToString().ToLowerInvariant();
        return this.negated
            ? text != this.condition
            : text == this.condition;
    }
}