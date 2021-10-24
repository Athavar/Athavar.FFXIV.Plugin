// <copyright file="RequireCommand.cs" company="Athavar">
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

    /// <summary>
    /// Implement the require command.
    /// </summary>
    internal class RequireCommand : BaseCommand
    {
        private readonly Regex requireCommand = new(@"^/require\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "require" };

        /// <inheritdoc/>
        public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var maxwait = ExtractMaxWait(ref step, 1000);

            var match = this.requireCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var effectName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower();

            var sheet = DalamudBinding.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.Status>()!;
            var effectIDs = sheet.Where(row => row.Name.RawString.ToLower() == effectName).Select(row => row.RowId).ToList();

            var hasEffect = await LinearWaitFor(
                250,
                Convert.ToInt32(maxwait.TotalMilliseconds),
                () => DalamudBinding.ClientState.LocalPlayer?.StatusList.Select(se => se.StatusId).ToList().Intersect(effectIDs).Any() ?? false,
                cancellationToken);

            if (!hasEffect)
            {
                throw new EffectNotPresentError($"Effect \"{effectName}\" not present");
            }

            return wait;
        }
    }
}
