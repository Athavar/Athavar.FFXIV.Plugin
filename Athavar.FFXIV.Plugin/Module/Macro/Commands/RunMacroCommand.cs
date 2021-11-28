// <copyright file="RunMacroCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;

/// <summary>
///     Implement the runmacro command.
/// </summary>
internal class RunMacroCommand : BaseCommand
{
    private readonly Regex runMacroCommand = new(@"^/runmacro\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    ///     Initializes a new instance of the <see cref="RunMacroCommand" /> class.
    /// </summary>
    /// <param name="macroManager"><see cref="MacroManager" /> added by DI.</param>
    public RunMacroCommand(MacroManager macroManager)
        : base(macroManager)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<string> CommandAliases => new[] { "runmacro" };

    /// <inheritdoc />
    public override Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
    {
        var match = this.runMacroCommand.Match(step);
        if (!match.Success)
        {
            throw new InvalidMacroOperationException("Syntax error");
        }

        var macroName = match.Groups["name"].Value.Trim(' ', '"', '\'');
        var macroNode = this.Manager.Configuration.GetAllNodes().FirstOrDefault(macro => macro.Name == macroName) as MacroNode;
        if (macroNode == default)
        {
            throw new InvalidMacroOperationException($"Unknown macro \"{macroName}\"");
        }

        this.Manager.RunMacro(macroNode, 0);

        return Task.FromResult(wait);
    }
}