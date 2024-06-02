// <copyright file="InteractCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Common.Extension;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;

[MacroCommand("interact", null, "Interact with new nearest selectable target with the given name.", ["wait"], ["/interact Summoning Bell"], RequireLogin = true)]
internal class InteractCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/interact\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string targetName;

    public InteractCommand(string text, string targetName, WaitModifier waitMod)
        : base(text, waitMod)
        => this.targetName = targetName.ToLowerInvariant();

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static InteractCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = match.ExtractAndUnquote("name");

        return new InteractCommand(text, nameValue, waitModifier);
    }

    /// <inheritdoc/>
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        this.Logger.Debug($"Executing: {this.Text}");

        if (!this.CommandInterface.IsTargetInReach(this.targetName))
        {
            throw new MacroCommandError($"Could not find target {this.targetName} in interaction range");
        }

        if (!this.CommandInterface.InteractWithTarget(this.targetName))
        {
            throw new MacroCommandError($"Fail to interact with target {this.targetName}");
        }

        await this.PerformWait(token);
    }
}