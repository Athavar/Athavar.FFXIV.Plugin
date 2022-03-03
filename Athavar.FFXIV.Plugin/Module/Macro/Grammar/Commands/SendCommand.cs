// <copyright file="SendCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using static Native;

/// <summary>
///     The /send command.
/// </summary>
internal class SendCommand : MacroCommand
{
    private static readonly Regex Regex = new(@"^/send\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly KeyCode[] vkCodes;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SendCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="vkCodes">KeyCode codes.</param>
    /// <param name="waitMod">Wait value.</param>
    private SendCommand(string text, KeyCode[] vkCodes, WaitModifier waitMod)
        : base(text, waitMod)
        =>
        this.vkCodes = vkCodes;

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static SendCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValues = ExtractAndUnquote(match, "name").Split(' ');

        List<KeyCode> vkCodes = new();
        foreach (var nameValue in nameValues)
        {
            if (!Enum.TryParse<KeyCode>(nameValue, true, out var vkCode))
            {
                throw new MacroCommandError($"Invalid key code '{nameValue}'");
            }

            vkCodes.Add(vkCode);
        }

        return new SendCommand(text, vkCodes.ToArray(), waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        var mWnd = Process.GetCurrentProcess().MainWindowHandle;

        foreach (var keyCode in this.vkCodes)
        {
            KeyDown(mWnd, keyCode);
        }

        await Task.Delay(15, token);

        foreach (var keyCode in this.vkCodes)
        {
            KeyUp(mWnd, keyCode);
        }

        await this.PerformWait(token);
    }
}