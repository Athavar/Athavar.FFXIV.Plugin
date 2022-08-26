// <copyright file="MacroParser.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar;

using System.Collections.Generic;
using System.IO;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

/// <summary>
///     A macro parser.
/// </summary>
internal static class MacroParser
{
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