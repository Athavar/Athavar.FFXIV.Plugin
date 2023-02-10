// <copyright file="WaitCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Globalization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Dalamud.Logging;

/// <summary>
///     The /wait command.
/// </summary>
[MacroCommand("wait", null, "The same as the wait modifier, but as a command.", new string[0], new[] { "/wait 1-5" })]
internal class WaitCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/wait\s+(?<wait>\d+(?:\.\d+)?)(?:-(?<until>\d+(?:\.\d+)?))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    ///     Initializes a new instance of the <see cref="WaitCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="waitUntil">WaitUntil value.</param>
    private WaitCommand(string text, int wait, int waitUntil)
        : base(text, wait, waitUntil)
    {
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static WaitCommand Parse(string text)
    {
        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var waitGroup = match.Groups["wait"];
        var waitValue = waitGroup.Value;
        var wait = (int)(float.Parse(waitValue, CultureInfo.InvariantCulture) * 1000);

        var untilGroup = match.Groups["until"];
        var untilValue = untilGroup.Success ? untilGroup.Value : "0";
        var until = (int)(float.Parse(untilValue, CultureInfo.InvariantCulture) * 1000);

        if (wait > until && until > 0)
        {
            throw new ArgumentException("Wait value cannot be lower than the until value");
        }

        return new WaitCommand(text, wait, until);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        await this.PerformWait(token);
    }
}