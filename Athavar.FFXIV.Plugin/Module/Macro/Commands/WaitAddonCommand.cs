// <copyright file="WaitAddonCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using FFXIVClientStructs.FFXIV.Component.GUI;

    /// <summary>
    /// Implement the waitaddon command.
    /// </summary>
    internal class WaitAddonCommand : BaseCommand
    {
        private readonly Regex waitAddonCommand = new(@"^/waitaddon\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "waitaddon" };

        /// <inheritdoc/>
        public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var maxwait = ExtractMaxWait(ref step, 5000);

            var match = this.waitAddonCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var addonPtr = IntPtr.Zero;
            var addonName = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' });

            var isVisible = await LinearWaitFor(
                500,
                Convert.ToInt32(maxwait.TotalMilliseconds),
                () =>
                {
                    addonPtr = DalamudBinding.GameGui.GetAddonByName(addonName, 1);
                    if (addonPtr != IntPtr.Zero)
                    {
                        unsafe
                        {
                            var addon = (AtkUnitBase*)addonPtr;
                            return addon->IsVisible && addon->UldManager.LoadedState == 3;
                        }
                    }

                    return false;
                },
                cancellationToken);

            if (addonPtr == IntPtr.Zero)
            {
                throw new InvalidMacroOperationException($"Could not find Addon \"{addonName}\"");
            }

            if (!isVisible)
            {
                throw new InvalidMacroOperationException($"Addon \"{addonName}\" not visible");
            }

            return wait;
        }
    }
}
