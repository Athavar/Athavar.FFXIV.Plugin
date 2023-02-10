// <copyright file="LoopCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Globalization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Dalamud.Logging;

/// <summary>
///     The /loop command.
/// </summary>
[MacroCommand("loop", null, "Loop the current macro forever, or a certain amount of times.", new[] { "echo", "wait" }, new[] { "/loop", "/loop 5" })]
internal class LoopCommand : MacroCommand
{
    private const int MaxLoops = int.MaxValue;
    private static readonly Regex Regex = new(@"^/loop(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly EchoModifier echoMod;
    private readonly int startingLoops;
    private int loopsRemaining;

    /// <summary>
    ///     Initializes a new instance of the <see cref="LoopCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="loopCount">Loop count.</param>
    /// <param name="waitMod">Wait value.</param>
    /// <param name="echo">Echo value.</param>
    private LoopCommand(string text, int loopCount, WaitModifier waitMod, EchoModifier echo)
        : base(text, waitMod)
    {
        this.loopsRemaining = loopCount >= 0 ? loopCount : MaxLoops;
        this.startingLoops = this.loopsRemaining;

        if (Configuration.LoopTotal && this.loopsRemaining != 0 && this.loopsRemaining != MaxLoops)
        {
            this.loopsRemaining -= 1;
        }

        this.echoMod = echo;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static LoopCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = EchoModifier.TryParse(ref text, out var echoModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var countGroup = match.Groups["count"];
        var countValue = countGroup.Success
            ? int.Parse(countGroup.Value, CultureInfo.InvariantCulture)
            : int.MaxValue;

        return new LoopCommand(text, countValue, waitModifier, echoModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        if (this.loopsRemaining != MaxLoops)
        {
            if (this.echoMod.PerformEcho || Configuration.LoopEcho)
            {
                ChatManager.PrintChat(this.loopsRemaining == 0 ? "No loops remaining" : $"{this.loopsRemaining} {(this.loopsRemaining == 1 ? "loop" : "loops")} remaining");
            }

            this.loopsRemaining--;

            if (this.loopsRemaining < 0)
            {
                this.loopsRemaining = this.startingLoops;
                return;
            }
        }

        macro.Loop();
        MacroManager.LoopCheckForPause();
        MacroManager.LoopCheckForStop();

        await this.PerformWait(token);
    }
}