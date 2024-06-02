// <copyright file="MacroModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Macro;

using System.Text;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Macro.Grammar.Commands;
using Athavar.FFXIV.Plugin.Macro.Grammar.Modifiers;
using Athavar.FFXIV.Plugin.Macro.Managers;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Dalamud.Game.Command;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements the macro module.
/// </summary>
[Module(ModuleName, ModuleConfigurationType = typeof(MacroConfiguration), HasTab = true)]
internal sealed class MacroModule : Module<MacroConfigTab, MacroConfiguration>
{
    /// <summary>
    ///     The name of the module.
    /// </summary>
    internal const string ModuleName = "Macro";

    private const string MacroCommandName = "/pmacro";

    private readonly IServiceProvider provider;
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroModule"/> class.
    /// </summary>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="configuration"><see cref="MacroConfiguration"/> added by DI.</param>
    public MacroModule(IServiceProvider provider, MacroConfiguration configuration)
        : base(configuration)
    {
        this.provider = provider;
        MacroCommand.SetServiceProvider(provider);
        MacroModifier.SetServiceProvider(provider);
        ActiveMacro.SetServiceProvider(provider);
        this.dalamudServices = provider.GetRequiredService<IDalamudServices>();
        this.chatManager = provider.GetRequiredService<IChatManager>();

        this.dalamudServices.PluginLogger.Debug("MMEntries: {0}", configuration.GetAllNodes().Count());

        this.dalamudServices.CommandManager.AddHandler(MacroCommandName, new CommandInfo(this.OnChatCommand)
        {
            HelpMessage = "Commands of the macro module.",
            ShowInHelp = true,
        });
        this.dalamudServices.PluginLogger.Debug("Module 'Macro' init");
    }

    /// <inheritdoc/>
    public override string Name => ModuleName;

    /// <inheritdoc/>
    public override void Dispose()
    {
        this.dalamudServices.CommandManager.RemoveHandler(MacroCommandName);
        base.Dispose();
    }

    protected override MacroConfigTab InitTab() => ActivatorUtilities.CreateInstance<MacroConfigTab>(this.provider);

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();
        var macroManager = this.provider.GetRequiredService<MacroManager>();

        if (arguments == string.Empty)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Help menu");
            sb.AppendLine($"{MacroCommandName} run <name> - Run the macro with the name.");
            sb.AppendLine($"{MacroCommandName} run loop <count> <name> - Run the macro with the name in a loop.");
            sb.AppendLine($"{MacroCommandName} pause - Pausing the current running macro.");
            sb.AppendLine($"{MacroCommandName} pause loop - Pausing the current running macro at the next /loop.");
            sb.AppendLine($"{MacroCommandName} stop - Stop the current running marco.");
            sb.AppendLine($"{MacroCommandName} stop loop - Stop the current running marco at the next /loop..");
            sb.AppendLine($"{MacroCommandName} help - Toggle the help-window.");
            this.chatManager.PrintChat(sb.ToString());
        }
        else if (arguments.StartsWith("run "))
        {
            arguments = arguments[4..].Trim();

            var loopCount = 0u;
            if (arguments.StartsWith("loop "))
            {
                arguments = arguments[5..].Trim();
                var nextSpace = arguments.IndexOf(' ');
                if (nextSpace == -1)
                {
                    this.chatManager.PrintErrorMessage("Could not determine loop count");
                    return;
                }

                if (!uint.TryParse(arguments[..nextSpace], out loopCount))
                {
                    this.chatManager.PrintErrorMessage("Could not parse loop count");
                    return;
                }

                arguments = arguments[(nextSpace + 1)..].Trim();
            }

            var macroName = arguments.Trim('"');
            var nodes = this.ModuleConfig.GetAllNodes()
               .OfType<MacroNode>()
               .Where(node => node.Name.Trim() == macroName)
               .ToArray();

            if (nodes.Length == 0)
            {
                this.chatManager.PrintErrorMessage("No macros match that name");
                return;
            }

            if (nodes.Length > 1)
            {
                this.chatManager.PrintErrorMessage("More than one macro matches that name");
                return;
            }

            var node = nodes[0];

            if (loopCount > 0)
            {
                // Clone a new node so the modification doesn't save.
                node = new MacroNode
                {
                    Name = node.Name,
                    Contents = node.Contents,
                };

                var lines = node.Contents.Split('\r', '\n');
                for (var i = lines.Length - 1; i >= 0; i--)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("/loop"))
                    {
                        var parts = line.Split()
                           .Where(s => !string.IsNullOrEmpty(s))
                           .ToArray();

                        var echo = line.Contains("<echo>") ? "<echo>" : string.Empty;
                        lines[i] = $"/loop {loopCount} {echo}";
                        node.Contents = string.Join('\n', lines);
                        this.chatManager.PrintChat($"Running macro \"{macroName}\" {loopCount} times");
                        break;
                    }
                }
            }
            else
            {
                this.chatManager.PrintChat($"Running macro \"{macroName}\"");
            }

            macroManager.EnqueueMacro(node);
        }
        else
        {
            switch (arguments)
            {
                case "pause":
                    this.chatManager.PrintChat("Pausing");
                    macroManager.Pause();
                    break;
                case "pause loop":
                    this.chatManager.PrintChat("Pausing at next /loop");
                    macroManager.Pause(true);
                    return;
                case "resume":
                    this.chatManager.PrintChat("Resuming");
                    macroManager.Resume();
                    break;
                case "stop":
                    this.chatManager.PrintChat("Stopping");
                    macroManager.Stop();
                    break;
                case "stop loop":
                    this.chatManager.PrintChat("Stopping at next /loop");
                    macroManager.Stop(true);
                    return;
                case "help":
                    this.provider.GetRequiredService<MacroHelpWindow>().Toggle();
                    break;
            }
        }
    }
}