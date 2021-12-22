﻿// <copyright file="RunMacroCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     The /runmacro command.
/// </summary>
internal class RunMacroCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/runmacro\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string macroName;

    private readonly MacroConfiguration configuration;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RunMacroCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="macroName">Macro name.</param>
    /// <param name="wait">Wait value.</param>
    private RunMacroCommand(string text, string macroName, WaitModifier wait)
        : base(text, wait)
    {
        this.macroName = macroName;

        this.configuration = ServiceProvider.GetRequiredService<Configuration>().Macro!;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static RunMacroCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = ExtractAndUnquote(match, "name");

        return new RunMacroCommand(text, nameValue, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        var macroNode = this.configuration
                            .GetAllNodes().OfType<MacroNode>()
                            .FirstOrDefault(macro => macro.Name == this.macroName);

        if (macroNode == default)
        {
            throw new MacroCommandError("No macro with that name");
        }

        macroManager.EnqueueMacro(macroNode);

        await this.PerformWait(token);
    }
}