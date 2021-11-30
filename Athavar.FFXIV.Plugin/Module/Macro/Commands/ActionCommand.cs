// <copyright file="ActionCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;
using Dalamud.Game.ClientState.Conditions;
using Lumina.Excel.GeneratedSheets;
using Action = Lumina.Excel.GeneratedSheets.Action;

/// <summary>
///     Implement the action command.
/// </summary>
internal class ActionCommand : BaseCommand
{
    private readonly Regex actionCommand = new(@"^/(ac|action)\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly List<string> craftingActionNames = new();

    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ActionCommand" /> class.
    /// </summary>
    /// <param name="macroManager"><see cref="MacroManager" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="chatManager"><see cref="IChatManager" /> added by DI.</param>
    public ActionCommand(MacroManager macroManager, IDalamudServices dalamudServices, IChatManager chatManager)
        : base(macroManager)
    {
        this.dalamudServices = dalamudServices;
        this.chatManager = chatManager;
        this.PopulateCraftingActionNames();
    }

    /// <inheritdoc />
    public override IEnumerable<string> CommandAliases => new[] { "ac", "action" };

    /// <inheritdoc />
    public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
    {
        var match = this.actionCommand.Match(step);
        if (!match.Success)
        {
            throw new InvalidMacroOperationException("Syntax error");
        }

        var actionName = match.Groups["name"].Value.Trim(' ', '"', '\'').ToLower();

        if (this.IsCraftingAction(actionName))
        {
            const int delayWait = 500;

            this.chatManager.SendMessage(step);

            await Task.Delay(Math.Max((int)wait.TotalMilliseconds - delayWait, 0), cancellationToken);
            wait = TimeSpan.Zero;

            // wait for crafting condition flag to exit.
            await Task.Delay(delayWait, cancellationToken);
            while (this.dalamudServices.Condition[ConditionFlag.Crafting40])
            {
                await Task.Delay(10, cancellationToken);
            }
        }
        else
        {
            this.chatManager.SendMessage(step);
        }

        return wait;
    }

    private void PopulateCraftingActionNames()
    {
        var actions = this.dalamudServices.DataManager.GetExcelSheet<Action>()!;
        foreach (var row in actions)
        {
            var job = row.ClassJob?.Value?.ClassJobCategory?.Value;
            if (job != null && (job.CRP || job.BSM || job.ARM || job.GSM || job.LTW || job.WVR || job.ALC || job.CUL))
            {
                var name = row.Name.RawString.Trim(' ', '"', '\'').ToLower();
                if (!this.craftingActionNames.Contains(name))
                {
                    this.craftingActionNames.Add(name);
                }
            }
        }

        var craftActions = this.dalamudServices.DataManager.GetExcelSheet<CraftAction>()!;
        foreach (var row in craftActions)
        {
            var name = row.Name.RawString.Trim(' ', '"', '\'').ToLower();
            if (name.Length > 0 && !this.craftingActionNames.Contains(name))
            {
                this.craftingActionNames.Add(name);
            }
        }
    }

    private bool IsCraftingAction(string name) => this.craftingActionNames.Contains(name.Trim(' ', '"', '\'').ToLower());
}