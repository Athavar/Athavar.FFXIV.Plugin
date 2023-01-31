// <copyright file="CheckCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

using System.Text.RegularExpressions;
using Athavar.FFXIV.Plugin.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implement the check command.
/// </summary>
[MacroCommand("check")]
internal class CheckCommand : MacroCommand
{
    private const int StatusCheckMaxWait = 1000;
    private const int StatusCheckInterval = 250;

    private static readonly Regex Regex = new(@"^/check\s+(?<category>.*?)\s+\((?<condition>.*?)\)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static ConditionCheck? conditionCheck;

    private readonly ConditionCheck.Category category;
    private readonly string condition;
    private readonly int maxWait;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheckCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="category">Condition category.</param>
    /// <param name="condition">Condition value.</param>
    /// <param name="waitMod">Wait value.</param>
    /// <param name="maxWait">MaxWait value.</param>
    private CheckCommand(string text, ConditionCheck.Category category, string condition, WaitModifier waitMod, MaxWaitModifier maxWait)
        : base(text, waitMod)
    {
        this.category = category;
        this.condition = condition;

        conditionCheck ??= ServiceProvider.GetRequiredService<ConditionCheck>();

        this.maxWait = maxWait.Wait == 0
            ? StatusCheckMaxWait
            : maxWait.Wait;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static CheckCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var categoryValue = ExtractAndUnquote(match, "category");
        var conditionValue = ExtractAndUnquote(match, "condition");

        if (!Enum.TryParse<ConditionCheck.Category>(categoryValue, true, out var parseCategory))
        {
            throw new MacroSyntaxError($"Unknown Condition category '{categoryValue}'");
        }

        return new CheckCommand(text, parseCategory, conditionValue, waitModifier, maxWaitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        var fulfilled = await this.LinearWait(StatusCheckInterval, this.maxWait, this.IsConditionFulfilled, token);

        if (!fulfilled)
        {
            throw new MacroCommandError("Condition not fulfilled");
        }

        await this.PerformWait(token);
    }

    private bool IsConditionFulfilled() => conditionCheck!.Check(this.category, this.condition);
}