// <copyright file="InteractCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;

using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

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

        var nameValue = ExtractAndUnquote(match, "name");

        return new InteractCommand(text, nameValue, waitModifier);
    }

    /// <inheritdoc />
    public override async Task Execute(ActiveMacro macro, CancellationToken token)
    {
        PluginLog.Debug($"Executing: {this.Text}");
        var target = DalamudServices.ObjectTable.FirstOrDefault(obj => obj.Name.TextValue.ToLowerInvariant() == this.targetName);

        if (target == default)
        {
            throw new MacroCommandError("Could not find target");
        }

        // backup current focus target.
        var currentFocusTarget = DalamudServices.TargetManager.FocusTarget;

        DalamudServices.TargetManager.SetFocusTarget(target);
        ServiceProvider.GetService<IClick>()?.TrySendClick("focus_interact");
        DalamudServices.TargetManager.SetFocusTarget(currentFocusTarget);

        await this.PerformWait(token);
    }
}