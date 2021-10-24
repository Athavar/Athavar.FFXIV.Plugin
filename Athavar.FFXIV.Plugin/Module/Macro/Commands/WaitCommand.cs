// <copyright file="WaitCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Implement the wait command.
    /// </summary>
    internal class WaitCommand : BaseCommand
    {
        private readonly Regex waitCommand = new(@"^/wait\s+(?<time>\d+(?:\.\d+)?)(?:-(?<maxtime>\d+(?:\.\d+)?))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "wait" };

        /// <inheritdoc/>
        public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var match = this.waitCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var waitTime = TimeSpan.Zero;
            var waitMatch = match.Groups["time"];
            if (waitMatch.Success && double.TryParse(waitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double seconds))
            {
                // PluginLog.Debug($"Wait is {waitTime.TotalMilliseconds}ms");
                waitTime = TimeSpan.FromSeconds(seconds);
            }

            var maxWaitMatch = match.Groups["maxtime"];
            if (maxWaitMatch.Success && double.TryParse(maxWaitMatch.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out double maxSeconds))
            {
                var rand = new Random();

                var maxWaitTime = TimeSpan.FromSeconds(maxSeconds);
                var diff = rand.Next((int)maxWaitTime.TotalMilliseconds - (int)waitTime.TotalMilliseconds);

                // PluginLog.Debug($"Wait (variable) is now {waitTime.TotalMilliseconds}ms");
                waitTime = TimeSpan.FromMilliseconds((int)waitTime.TotalMilliseconds + diff);
            }

            await Task.Delay(waitTime, cancellationToken);

            return wait;
        }
    }
}
