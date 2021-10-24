// <copyright file="LoopCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using static Athavar.FFXIV.Plugin.Module.Macro.MacroManager;

    /// <summary>
    /// Implement the loop command.
    /// </summary>
    internal class LoopCommand : BaseCommand
    {
        private readonly Regex loopCommand = new(@"^/loop(?: (?<count>\d+))?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "loop" };

        /// <inheritdoc/>
        public override Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            if (this.Manager.State == LoopState.Cancel)
            {
                // Skip loops in canceled state.
                return Task.FromResult(wait);
            }

            var match = this.loopCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var countMatch = match.Groups["count"];
            if (!countMatch.Success)
            {
                macro.StepIndex = -1;
            }
            else if (countMatch.Success && int.TryParse(countMatch.Value, out var loopMax) && macro.LoopCount < loopMax)
            {
                macro.StepIndex = -1;
                macro.LoopCount++;
            }
            else
            {
                macro.LoopCount = 0;
            }

            return Task.FromResult(wait);
        }
    }
}
