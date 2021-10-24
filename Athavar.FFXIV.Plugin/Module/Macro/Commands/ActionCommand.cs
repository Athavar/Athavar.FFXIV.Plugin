// <copyright file="ActionCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implement the action command.
    /// </summary>
    internal class ActionCommand : BaseCommand
    {
        private readonly Regex actionCommand = new(@"^/(ac|action)\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly List<string> craftingActionNames = new();

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "ac", "action" };

        /// <inheritdoc/>
        public override void Init(MacroManager manager)
        {
            base.Init(manager);
            this.PopulateCraftingActionNames();
        }

        /// <inheritdoc/>
        public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var match = this.actionCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var actionName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();

            if (this.IsCraftingAction(actionName))
            {
                const int delayWait = 500;

                this.Manager.ChatManager.SendMessage(step);

                await Task.Delay(Math.Max((int)wait.TotalMilliseconds - delayWait, 0), cancellationToken);
                wait = TimeSpan.Zero;

                // wait for crafting condition flag to exit.
                await Task.Delay(delayWait, cancellationToken);
                while (DalamudBinding.Condition[Dalamud.Game.ClientState.Conditions.ConditionFlag.Crafting40])
                {
                    await Task.Delay(10, cancellationToken);
                }
            }
            else
            {
                this.Manager.ChatManager.SendMessage(step);
            }

            return wait;
        }

        private void PopulateCraftingActionNames()
        {
            var actions = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Action>()!;
            foreach (var row in actions)
            {
                var job = row.ClassJob?.Value?.ClassJobCategory?.Value;
                if (job != null && (job.CRP || job.BSM || job.ARM || job.GSM || job.LTW || job.WVR || job.ALC || job.CUL))
                {
                    var name = row.Name.RawString.Trim(new char[] { ' ', '"', '\'' }).ToLower();
                    if (!this.craftingActionNames.Contains(name))
                    {
                        this.craftingActionNames.Add(name);
                    }
                }
            }

            var craftActions = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.CraftAction>()!;
            foreach (var row in craftActions)
            {
                var name = row.Name.RawString.Trim(new char[] { ' ', '"', '\'' }).ToLower();
                if (name.Length > 0 && !this.craftingActionNames.Contains(name))
                {
                    this.craftingActionNames.Add(name);
                }
            }
        }

        private bool IsCraftingAction(string name) => this.craftingActionNames.Contains(name.Trim(new char[] { ' ', '"', '\'' }).ToLower());
    }
}
