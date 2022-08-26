// <copyright file="WaitAddonCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Component.GUI;

/// <summary>
///     The /waitaddon command.
/// </summary>
internal class WaitAddonCommand : MacroCommand
{
    private const int AddonCheckMaxWait = 5000;
    private const int AddonCheckInterval = 50;

    private static readonly Regex Regex = new(@"^/waitaddon\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly string addonName;
    private readonly int maxWait;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WaitAddonCommand" /> class.
    /// </summary>
    /// <param name="text">Original text.</param>
    /// <param name="addonName">Addon name.</param>
    /// <param name="waitMod">Wait value.</param>
    /// <param name="maxWait">MaxWait value.</param>
    private WaitAddonCommand(string text, string addonName, WaitModifier waitMod, MaxWaitModifier maxWait)
        : base(text, waitMod)
    {
        this.addonName = addonName;
        this.maxWait = maxWait.Wait == 0
            ? AddonCheckMaxWait
            : maxWait.Wait;
    }

    /// <summary>
    ///     Parse the text as a command.
    /// </summary>
    /// <param name="text">Text to parse.</param>
    /// <returns>A parsed command.</returns>
    public static WaitAddonCommand Parse(string text)
    {
        _ = WaitModifier.TryParse(ref text, out var waitModifier);
        _ = MaxWaitModifier.TryParse(ref text, out var maxWaitModifier);

        var match = Regex.Match(text);
        if (!match.Success)
        {
            throw new MacroSyntaxError(text);
        }

        var nameValue = ExtractAndUnquote(match, "name");

        return new WaitAddonCommand(text, nameValue, waitModifier, maxWaitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");

        var (addonPtr, isVisible) = await this.LinearWait(AddonCheckInterval, this.maxWait, this.IsAddonVisible, token);

        if (addonPtr == IntPtr.Zero)
        {
            throw new MacroCommandError("Addon not found");
        }

        if (!isVisible)
        {
            throw new MacroCommandError("Addon not visible");
        }

        await this.PerformWait(token);
    }

    private unsafe (IntPtr Addon, bool IsVisible) IsAddonVisible()
    {
        var addonPtr = DalamudServices.GameGui.GetAddonByName(this.addonName, 1);
        if (addonPtr == IntPtr.Zero)
        {
            return (addonPtr, false);
        }

        var addon = (AtkUnitBase*)addonPtr;
        if (!addon->IsVisible || addon->UldManager.LoadedState != AtkLoadState.Loaded)
        {
            return (addonPtr, false);
        }

        return (addonPtr, true);
    }
}