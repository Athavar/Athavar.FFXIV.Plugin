// <copyright file="TargetCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Dalamud.Game.ClientState.Objects.Types;

    /// <summary>
    /// Implement the target command.
    /// </summary>
    internal class TargetCommand : BaseCommand
    {
        private readonly Regex targetCommand = new(@"^/target\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "target" };

        /// <inheritdoc/>
        public override Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var match = this.targetCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var actorName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();
            GameObject? npc = null;
            try
            {
                npc = DalamudBinding.ObjectTable.Where(actor => actor.Name.TextValue.ToLower() == actorName).First();
            }
            catch (InvalidOperationException)
            {
                throw new InvalidMacroOperationException($"Unknown actor \"{actorName}\"");
            }

            if (npc != null)
            {
                DalamudBinding.TargetManager.SetTarget(npc);
            }

            return Task.FromResult(wait);
        }
    }
}
