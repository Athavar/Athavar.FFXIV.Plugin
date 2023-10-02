// <copyright file="GateCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Globalization;
using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;

/// <summary>
///     The /craft command.
/// </summary>
[MacroCommand("craft", "gate", "Similar to loop but used at the start of a macro with an infinite /loop at the end. Allows a certain amount of executions before stopping the macro.", new[] { "echo", "wait" }, new[] { "/craft 10" }, RequireLogin = true)]
internal class GateCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/(craft|gate)(?:\s+(?<count>\d+))?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly EchoModifier echoMod;
    private readonly int startingCrafts;
    private int craftsRemaining;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GateCommand"/> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="craftCount">Craft count.</param>
    /// <param name="wait">Wait value.</param>
    /// <param name="echo">Echo value.</param>
    private GateCommand(string text, int craftCount, WaitModifier wait, EchoModifier echo)
        : base(text, wait)
    {
        this.startingCrafts = this.craftsRemaining = craftCount;
        this.echoMod = echo;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static GateCommand Parse(string text)
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

        return new GateCommand(text, countValue, waitModifier, echoModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        this.Logger.Debug($"Executing: {this.Text}");

        if (this.echoMod.PerformEcho || this.Configuration.LoopEcho)
        {
            if (this.craftsRemaining == 0)
            {
                this.ChatManager.PrintChat("No crafts remaining");
            }
            else
            {
                var noun = this.craftsRemaining == 1 ? "craft" : "crafts";
                this.ChatManager.PrintChat($"{this.craftsRemaining} {noun} remaining");
            }
        }

        this.craftsRemaining--;

        await this.PerformWait(token);

        if (this.craftsRemaining < 0)
        {
            this.craftsRemaining = this.startingCrafts;
            throw new GateComplete();
        }
    }
}