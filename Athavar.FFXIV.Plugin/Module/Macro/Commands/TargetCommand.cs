// <copyright file="TargetCommand.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro.Commands;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Exceptions;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;
using Dalamud.Game.ClientState.Objects.Types;

/// <summary>
///     Implement the target command.
/// </summary>
internal class TargetCommand : BaseCommand
{
    private readonly Regex targetCommand = new(@"^/target\s+(?<name>.*?)\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly IDalamudServices dalamudServices;

    /// <summary>
    ///     Initializes a new instance of the <see cref="TargetCommand" /> class.
    /// </summary>
    /// <param name="macroManager"><see cref="MacroManager" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    public TargetCommand(MacroManager macroManager, IDalamudServices dalamudServices)
        : base(macroManager) =>
        this.dalamudServices = dalamudServices;

    /// <inheritdoc />
    public override IEnumerable<string> CommandAliases => new[] { "target" };

    /// <inheritdoc />
    public override Task<TimeSpan> Execute(string step, ActiveMacro macro, TimeSpan wait, CancellationToken cancellationToken)
    {
        var match = this.targetCommand.Match(step);
        if (!match.Success)
        {
            throw new InvalidMacroOperationException("Syntax error");
        }

        var actorName = match.Groups["name"].Value.Trim(' ', '"', '\'').ToLower();
        GameObject? npc = null;
        try
        {
            npc = this.dalamudServices.ObjectTable.First(actor => actor.Name.TextValue.ToLower() == actorName);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidMacroOperationException($"Unknown actor \"{actorName}\"");
        }

        this.dalamudServices.TargetManager.SetTarget(npc);

        return Task.FromResult(wait);
    }
}