// <copyright file="SendCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using static Athavar.FFXIV.Plugin.Native;

    /// <summary>
    /// Implement the loop command.
    /// </summary>
    internal class SendCommand : BaseCommand, IDisposable
    {
        private readonly Regex sendCommand = new(@"^/send\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly Process proc;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendCommand"/> class.
        /// </summary>
        public SendCommand()
        {
            this.proc = Process.GetCurrentProcess();
        }

        /// <inheritdoc/>
        public override IEnumerable<string> CommandAliase => new[] { "send" };

        /// <inheritdoc/>
        public void Dispose()
        {
            this.proc.Dispose();
        }

        /// <inheritdoc/>
        public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
        {
            var match = this.sendCommand.Match(step);
            if (!match.Success)
            {
                throw new InvalidMacroOperationException("Syntax error");
            }

            var vkNames = match.Groups["name"].Value.Trim(new char[] { ' ', '"', '\'' }).ToLower().Split(' ');

            var vkCodes = vkNames.Select(n => Enum.Parse<KeyCode>(n, true)).ToArray();
            if (vkCodes.Any(c => !Enum.IsDefined(c)))
            {
                throw new InvalidMacroOperationException($"Invalid virtual key");
            }
            else
            {
                var mWnd = this.proc.MainWindowHandle;

                for (int i = 0; i < vkCodes.Length; i++)
                {
                    Native.KeyDown(mWnd, vkCodes[i]);
                }

                await Task.Delay(15);

                for (int i = 0; i < vkCodes.Length; i++)
                {
                    Native.KeyUp(mWnd, vkCodes[i]);
                }
            }

            return wait;
        }
    }
}
