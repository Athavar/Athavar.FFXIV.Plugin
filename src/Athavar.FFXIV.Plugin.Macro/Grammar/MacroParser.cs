﻿// <copyright file="MacroParser.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro.Grammar;

using System.Reflection;
using Athavar.FFXIV.Plugin.Macro.Grammar.Commands;

/// <summary>
///     A macro parser.
/// </summary>
internal static class MacroParser
{
    private static readonly Dictionary<string, Func<string, MacroCommand>> Commands = new();

    static MacroParser()
    {
        List<(string Name, string? Alias, string Description, string[] Modifiers, string[] Examples)> data = new();

        var attribute = typeof(MacroCommandAttribute);
        var commands = typeof(MacroCommand).Assembly.GetTypes().Where(t => !t.IsAbstract && t.IsDefined(attribute, false)).Select(t => (Command: t, Description: (MacroCommandAttribute)t.GetCustomAttribute(attribute)!));

        foreach (var (command, description) in commands)
        {
            MacroCommand ParseAction(string t) => (MacroCommand)command.GetMethod("Parse", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, new[] { t });
            Commands.Add(description.Name, ParseAction);
            if (description.Alias is not null)
            {
                Commands.Add(description.Alias, ParseAction);
            }

            if (!description.Hidden)
            {
                data.Add((description.Name, description.Alias, description.Description, description.Modifiers, description.Examples));
            }
        }

        CommandData = data.ToArray();
    }

    internal static (string Name, string? Alias, string Description, string[] Modifiers, string[] Examples)[] CommandData { get; }

    /// <summary>
    ///     Parse a macro and return a series of executable statements.
    /// </summary>
    /// <param name="macroText">Macro to parse.</param>
    /// <returns>A series of executable statements.</returns>
    public static IEnumerable<MacroCommand> Parse(string macroText)
    {
        string? line;
        using var reader = new StringReader(macroText);

        while ((line = reader.ReadLine()) != null)
        {
            // Empty
            if (line.Trim().Length == 0)
            {
                continue;
            }

            // Comment
            if (line.StartsWith('#') || line.StartsWith("//"))
            {
                continue;
            }

            yield return ParseLine(line);
        }
    }

    /// <summary>
    ///     Parse a single macro line and return the appropriate command.
    /// </summary>
    /// <param name="line">Text to parse.</param>
    /// <returns>An executable statement.</returns>
    public static MacroCommand ParseLine(string line)
    {
        // Extract the slash command
        var firstSpace = line.IndexOf(' ');
        var commandText = firstSpace != -1
            ? line[..firstSpace]
            : line;

        commandText = commandText.ToLowerInvariant();

        if (commandText[0] == '/' && Commands.TryGetValue(commandText[1..], out var parser))
        {
            return parser(line);
        }

        return NativeCommand.Parse(line);
        return commandText switch
               {
                   "/ac" => ActionCommand.Parse(line),
                   "/action" => ActionCommand.Parse(line),
                   "/check" => CheckCommand.Parse(line),
                   "/click" => ClickCommand.Parse(line),
                   "/craft" => GateCommand.Parse(line),
                   "/gate" => GateCommand.Parse(line),
                   "/interact" => InteractCommand.Parse(line),
                   "/item" => ItemCommand.Parse(line),
                   "/loop" => LoopCommand.Parse(line),
                   "/recipe" => RecipeCommand.Parse(line),
                   "/require" => RequireCommand.Parse(line),
                   "/requirequality" => RequireQualityCommand.Parse(line),
                   "/requirerepair" => RequireRepairCommand.Parse(line),
                   "/requirespiritbond" => RequireSpiritbondCommand.Parse(line),
                   "/requirestats" => RequireStatsCommand.Parse(line),
                   "/runmacro" => RunMacroCommand.Parse(line),
                   "/send" => SendCommand.Parse(line),
                   "/target" => TargetCommand.Parse(line),
                   "/waitaddon" => WaitAddonCommand.Parse(line),
                   "/wait" => WaitCommand.Parse(line),
                   _ => NativeCommand.Parse(line),
               };
    }
}