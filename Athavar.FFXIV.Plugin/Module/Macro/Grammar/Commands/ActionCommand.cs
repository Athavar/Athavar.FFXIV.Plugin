﻿// <copyright file="ActionCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.GeneratedSheets;
using Microsoft.Extensions.DependencyInjection;
using Action = Lumina.Excel.GeneratedSheets.Action;

/// <summary>
///     The /action command.
/// </summary>
internal class ActionCommand : MacroCommand
{
    private const int SafeCraftMaxWait = 5000;

    private static readonly Regex Regex = new(@"^/(?:ac|action)\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly HashSet<string> CraftingActionNames = new();

    // For when quality is at 100%, skip it if the action is one of these.
    private static readonly HashSet<string> CraftingQualityActionNames = new();

    private readonly string actionName;
    private readonly UnsafeModifier unsafeMod;
    private readonly ConditionModifier conditionMod;

    static ActionCommand()
    {
        PopulateCraftingNames();
        PopulateCraftingQualityActionNames();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="actionName">Action name.</param>
    /// <param name="waitMod">Wait value.</param>
    /// <param name="unsafeMod">Perform the action safely.</param>
    /// <param name="conditionMod">Required crafting condition.</param>
    private ActionCommand(string text, string actionName, WaitModifier waitMod, UnsafeModifier unsafeMod, ConditionModifier conditionMod)
        : base(text, waitMod)
    {
        this.actionName = actionName.ToLowerInvariant();
        this.unsafeMod = unsafeMod;
        this.conditionMod = conditionMod;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static ActionCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = UnsafeModifier.TryParse(ref text, out var unsafeModifier);
        _ = ConditionModifier.TryParse(ref text, out var conditionModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = ExtractAndUnquote(match, "name");

        return new ActionCommand(text, nameValue, waitModifier, unsafeModifier, conditionModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        if (IsCraftingAction(this.actionName))
        {
            if (Configuration.CraftSkip)
            {
                if (IsNotCrafting())
                {
                    PluginLog.Debug($"Not crafting skip: {this.Text}");
                    await Task.Delay(10, token);
                    return;
                }

                if (HasMaxProgress())
                {
                    PluginLog.Debug($"Max progress skip: {this.Text}");
                    await Task.Delay(10, token);
                    return;
                }
            }

            if (!this.conditionMod.HasCondition())
            {
                PluginLog.Debug($"Condition skip: {this.Text}");
                return;
            }

            if (Configuration.QualitySkip && IsSkippableCraftingQualityAction(this.actionName) && HasMaxQuality())
            {
                PluginLog.Debug($"Max quality skip: {this.Text}");
                return;
            }

            const int delayWait = 500;

            ChatManager.SendMessage(this.Text);

            if (!Configuration.CraftWaitSkip)
            {
                await this.PerformWait(token);
            }

            // wait for crafting condition flag to exit.
            await Task.Delay(delayWait, token);
            while (DalamudServices.Condition[ConditionFlag.Crafting40])
            {
                await Task.Delay(10, token);
            }
        }
        else
        {
            ChatManager.SendMessage(this.Text);

            await this.PerformWait(token);
        }
    }

    private static bool IsCrafting() => DalamudServices.Condition[ConditionFlag.Crafting] && !DalamudServices.Condition[ConditionFlag.PreparingToCraft];

    private static bool IsNotCrafting() => !IsCrafting();

    private static bool IsCraftingAction(string name) => CraftingActionNames.Contains(name);

    private static bool IsSkippableCraftingQualityAction(string name) => CraftingQualityActionNames.Contains(name);

    private static unsafe bool HasMaxProgress()
    {
        var addon = DalamudServices.GameGui.GetAddonByName("Synthesis", 1);
        if (addon == IntPtr.Zero)
        {
            PluginLog.Debug("Could not find Synthesis addon");
            return false;
        }

        var addonPtr = (AddonSynthesis*)addon;
        var curProgress = GetNodeTextAsInt(addonPtr->CurrentProgress, "Could not parse progress number in the Synthesis addon");
        var maxProgress = GetNodeTextAsInt(addonPtr->MaxProgress, "Could not parse max progress number in the Synthesis addon");
        return curProgress == maxProgress;
    }

    private static unsafe bool HasMaxQuality()
    {
        var addon = DalamudServices.GameGui.GetAddonByName("Synthesis", 1);
        if (addon == IntPtr.Zero)
        {
            PluginLog.Debug("Could not find Synthesis addon");
            return false;
        }

        var addonPtr = (AddonSynthesis*)addon;

        var step = GetNodeTextAsInt(addonPtr->StepNumber, "Could not parse step number in the Synthesis addon");

        if (step <= 1)
        {
            return false;
        }

        var isCollectable = addonPtr->AtkUnitBase.UldManager.NodeList[34]->IsVisible;
        if (isCollectable)
        {
            var curQuality = GetNodeTextAsInt(addonPtr->CurrentQuality, "Could not parse current quality number in the Synthesis addon");
            var maxQuality = GetNodeTextAsInt(addonPtr->MaxQuality, "Could not parse max quality number number in the Synthesis addon");
            return curQuality == maxQuality;
        }

        var percentHq = GetNodeTextAsInt(addonPtr->HQPercentage, "Could not parse percent hq number in the Synthesis addon");
        return percentHq == 100;
    }

    private static void PopulateCraftingNames()
    {
        var dalamudServices = ServiceProvider.GetRequiredService<IDalamudServices>();
        var actions = dalamudServices.DataManager.GetExcelSheet<Action>()!;
        foreach (var row in actions)
        {
            var job = row.ClassJob?.Value?.ClassJobCategory?.Value;
            if (job == null)
            {
                continue;
            }

            if (job.CRP || job.BSM || job.ARM || job.GSM || job.LTW || job.WVR || job.ALC || job.CUL)
            {
                var name = row.Name.RawString.ToLowerInvariant();
                if (name.Length == 0)
                {
                    continue;
                }

                CraftingActionNames.Add(name);
            }
        }

        var craftActions = dalamudServices.DataManager.GetExcelSheet<CraftAction>()!;
        foreach (var row in craftActions)
        {
            var name = row.Name.RawString.ToLowerInvariant();
            if (name.Length == 0)
            {
                continue;
            }

            CraftingActionNames.Add(name);
        }
    }

    private static unsafe int GetNodeTextAsInt(AtkTextNode* node, string error)
    {
        if (node == null)
        {
            throw new NullReferenceException("Node is null");
        }

        var text = node->NodeText.ToString().ToLowerInvariant();
        PluginLog.Debug($"Text value: {text}");

        if (!int.TryParse(text, out var value))
        {
            throw new MacroCommandError(error);
        }

        return value;
    }

    private static void PopulateCraftingQualityActionNames()
    {
        var craftIDs = new uint[]
                       {
                           100002, 100016, 100031, 100046, 100061, 100076, 100091, 100106, // Basic Touch
                           100004, 100018, 100034, 100048, 100064, 100078, 100093, 100109, // Standard Touch
                           100411, 100412, 100413, 100414, 100415, 100416, 100417, 100418, // Advanced Touch
                           100128, 100129, 100130, 100131, 100132, 100133, 100134, 100135, // Precise Touch
                           100227, 100228, 100229, 100230, 100231, 100232, 100233, 100234, // Prudent Touch
                           100243, 100244, 100245, 100246, 100247, 100248, 100249, 100250, // Focused Touch
                           // 100283, 100284, 100285, 100286, 100287, 100288, 100289, 100290, // Trained Eye
                           100299, 100300, 100301, 100302, 100303, 100304, 100305, 100306, // Preparatory Touch
                           100339, 100340, 100341, 100342, 100343, 100344, 100345, 100346, // Byregot's Blessing
                           100355, 100356, 100357, 100358, 100359, 100360, 100361, 100362, // Hasty Touch
                           100435, 100436, 100437, 100438, 100439, 100440, 100441, 100442, // Trained Finesse
                           // 100387, 100388, 100389, 100390, 100391, 100392, 100393, 100394, // Reflect
                           // 100323, 100324, 100325, 100326, 100327, 100328, 100329, 100330, // Delicate Synthesis
                       };

        var actionIDs = new uint[]
                        {
                            19004, 19005, 19006, 19007, 19008, 19009, 19010, 19011, // Innovation
                            260, 261, 262, 263, 264, 265, 266, 267, // Great Strides
                        };

        var actions = DalamudServices.DataManager.GetExcelSheet<Action>()!;
        foreach (var actionId in actionIDs)
        {
            var row = actions.GetRow(actionId)!;
            var name = row.Name.RawString.ToLowerInvariant();
            CraftingQualityActionNames.Add(name);
        }

        var craftActions = DalamudServices.DataManager.GetExcelSheet<CraftAction>()!;
        foreach (var craftId in craftIDs)
        {
            var row = craftActions.GetRow(craftId)!;
            var name = row.Name.RawString.ToLowerInvariant();
            CraftingQualityActionNames.Add(name);
        }
    }
}