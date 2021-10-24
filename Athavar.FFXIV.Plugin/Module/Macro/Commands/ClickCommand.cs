// <copyright file="ClickCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using ClickLib;
    using Dalamud.Logging;

    /// <summary>
    /// Implement the click command.
    /// </summary>
    internal class ClickCommand : BaseCommand
    {
        private readonly Regex clickCommand = new(@"^/click\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "click" };

        /// <inheritdoc/>
        public override Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var match = this.clickCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var name = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();

            try
            {
                Click.SendClick(name);
            }
            catch (InvalidClickException ex)
            {
                PluginLog.Error(ex, $"Error while performing {name} click");
                throw new InvalidMacroOperationException($"Click error");
            }

            return Task.FromResult(wait);
        }
    }
}
