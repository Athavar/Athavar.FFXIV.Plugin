// <copyright file="CheckCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands;

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;

/// <summary>
///     Implement the check command.
/// </summary>
internal class CheckCommand : BaseCommand
{
    private readonly Regex checkCommand = new(@"^/check\s+(?<category>.*?)\s+\((?<condition>.*?)\)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Condition condition;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CheckCommand" /> class.
    /// </summary>
    /// <param name="macroManager"><see cref="MacroManager" /> added by DI.</param>
    /// <param name="condition"><see cref="Condition" /> added by DI.</param>
    public CheckCommand(MacroManager macroManager, Condition condition)
        : base(macroManager) =>
        this.condition = condition;

    /// <inheritdoc />
    public override IEnumerable<string> CommandAliases => new[] { "check" };

    /// <inheritdoc />
    public override async Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
    {
        var maxwait = ExtractMaxWait(ref step, 500);

        var match = this.checkCommand.Match(step);
        if (!match.Success)
        {
            throw new InvalidMacroOperationException("Syntax error");
        }

        var category = match.Groups["category"].Value.Trim(' ', '"', '\'').ToLowerInvariant();
        var condition = match.Groups["condition"].Value.Trim(' ', '"', '\'');

        if (!Enum.TryParse<Condition.Category>(category, true, out var parseCategory))
        {
            throw new InvalidMacroOperationException("Unknown category");
        }

        if (!await LinearWaitFor(
                250,
                Convert.ToInt32(maxwait.TotalMilliseconds),
                () => this.condition.ConditionCheck(parseCategory, condition),
                cancellationToken))
        {
            throw new ConditionNotFulfilledError($"Condition {condition} not fulfilled");
        }

        return wait;
    }
}