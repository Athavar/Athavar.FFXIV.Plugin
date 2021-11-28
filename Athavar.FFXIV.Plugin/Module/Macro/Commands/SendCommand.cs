// <copyright file="SendCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;
using static Native;

/// <summary>
///     Implement the loop command.
/// </summary>
internal class SendCommand : BaseCommand, IDisposable
{
    private readonly Regex sendCommand = new(@"^/send\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly Process proc;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SendCommand" /> class.
    /// </summary>
    /// <param name="macroManager"><see cref="MacroManager" /> added by DI.</param>
    public SendCommand(MacroManager macroManager)
        : base(macroManager) => this.proc = Process.GetCurrentProcess();

    /// <inheritdoc />
    public override IEnumerable<string> CommandAliases => new[] { "send" };

    /// <inheritdoc />
    public void Dispose() => this.proc.Dispose();

    /// <inheritdoc />
    public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
    {
        var match = this.sendCommand.Match(step);
        if (!match.Success)
        {
            throw new InvalidMacroOperationException("Syntax error");
        }

        var vkNames = match.Groups["name"].Value.Trim(' ', '"', '\'').ToLower().Split(' ');

        var vkCodes = vkNames.Select(n => Enum.Parse<KeyCode>(n, true)).ToArray();
        if (vkCodes.Any(c => !Enum.IsDefined(c)))
        {
            throw new InvalidMacroOperationException("Invalid virtual key");
        }

        var mWnd = this.proc.MainWindowHandle;

        for (var i = 0; i < vkCodes.Length; i++)
        {
            KeyDown(mWnd, vkCodes[i]);
        }

        await Task.Delay(15);

        for (var i = 0; i < vkCodes.Length; i++)
        {
            KeyUp(mWnd, vkCodes[i]);
        }

        return wait;
    }
}