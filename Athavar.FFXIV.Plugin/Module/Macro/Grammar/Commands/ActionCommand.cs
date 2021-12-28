// <copyright file="ActionCommand.cs" company="Athavar">
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

    private readonly string actionName;
    private readonly bool safely;
    private readonly string condition;
    private readonly bool conditionNegated;

    static ActionCommand() => PopulateCraftingNames();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="actionName">Action name.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="safely">Perform the action safely.</param>
    /// <param name="condition">Required crafting condition.</param>
    /// <param name="conditionNegated">Negate the condition check.</param>
    private ActionCommand(string text, string actionName, WaitModifier wait, bool safely, string condition, bool conditionNegated)
        : base(text, wait.Wait, wait.Until)
    {
        this.actionName = actionName.ToLowerInvariant();
        this.safely = safely;
        this.condition = condition.ToLowerInvariant();
        this.conditionNegated = conditionNegated;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static ActionCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        var hasUnsafe = UnsafeModifier.TryParse(ref text, out var _);
        _ = UnsafeModifier.TryParse(ref text, out var unsafeModifier);
        _ = ConditionModifier.TryParse(ref text, out var conditionModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = ExtractAndUnquote(match, "name");

        return new ActionCommand(text, nameValue, waitModifier, !unsafeModifier.IsUnsafe, conditionModifier.Condition, conditionModifier.Negated);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        if (IsCraftingAction(this.actionName))
        {
            if (!HasCondition(this.condition, this.conditionNegated))
            {
                PluginLog.Debug($"Condition skip: {this.Text}");
                return;
            }

            const int delayWait = 500;

            ChatManager.SendMessage(this.Text);

            await this.PerformWait(token);

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

    private static bool IsCraftingAction(string name)
        => CraftingActionNames.Contains(name);

    private static unsafe bool HasCondition(string condition, bool negated)
    {
        if (condition == string.Empty)
        {
            return true;
        }

        var addon = DalamudServices.GameGui.GetAddonByName("Synthesis", 1);
        if (addon == IntPtr.Zero)
        {
            throw new MacroCommandError("Could not find Synthesis addon");
        }

        var textPtrPtr = addon + 0x260;
        var textPtr = *(AtkTextNode**)textPtrPtr;

        var text = textPtr->NodeText.ToString().ToLowerInvariant();

        return negated
            ? text != condition
            : text == condition;
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
}