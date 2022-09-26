// <copyright file="MacroModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using System;
using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Modifiers;
using Athavar.FFXIV.Plugin.Module.Macro.Managers;
using Dalamud.Game.Command;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Implements the macro module.
/// </summary>
internal sealed class MacroModule : IModule, IDisposable
{
    /// <summary>
    ///     The name of the module.
    /// </summary>
    internal const string ModuleName = "Macro";

    private const string MacroCommandName = "/pmacro";

    private readonly MacroConfigTab configTab;
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly MacroManager macroManager;
    private readonly PluginWindow pluginWindow;
    private readonly MacroHelpWindow helpWindow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MacroModule" /> class.
    /// </summary>
    /// <param name="moduleManager"><see cref="Manager.ModuleManager" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public MacroModule(IModuleManager moduleManager, IServiceProvider provider)
    {
        MacroCommand.SetServiceProvider(provider);
        MacroModifier.SetServiceProvider(provider);
        ActiveMacro.SetServiceProvider(provider);
        this.dalamudServices = provider.GetRequiredService<IDalamudServices>();
        this.chatManager = provider.GetRequiredService<IChatManager>();
        this.macroManager = provider.GetRequiredService<MacroManager>();
        this.configTab = provider.GetRequiredService<MacroConfigTab>();
        this.Configuration = provider.GetRequiredService<Configuration>().Macro!;

        this.pluginWindow = provider.GetRequiredService<PluginWindow>();
        this.helpWindow = provider.GetRequiredService<MacroHelpWindow>();

        moduleManager.Register(this, this.Configuration.Enabled);

        PluginLog.LogDebug("Module 'Macro' add command");
        this.dalamudServices.CommandManager.AddHandler(MacroCommandName, new CommandInfo(this.OnChatCommand)
                                                                         {
                                                                             HelpMessage = "Commands of the macro module.",
                                                                             ShowInHelp = true,
                                                                         });
        PluginLog.LogDebug("Module 'Macro' init");
    }

    public string Name => ModuleName;

    /// <summary>
    ///     Gets the configuration.
    /// </summary>
    internal MacroConfiguration Configuration { get; }

    /// <inheritdoc />
    public void Draw() => this.configTab.DrawTab();

    public void Enable(bool state = true) => this.Configuration.Enabled = state;

    /// <inheritdoc />
    public void Dispose() => this.dalamudServices.CommandManager.RemoveHandler(MacroCommandName);

    private void OnChatCommand(string command, string arguments)
    {
        arguments = arguments.Trim();

        if (arguments == string.Empty)
        {
            this.pluginWindow.Toggle();
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
            var nodes = this.Configuration.GetAllNodes()
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

            this.macroManager.EnqueueMacro(node);
        }
        else
        {
            switch (arguments)
            {
                case "pause":
                    this.chatManager.PrintChat("Pausing");
                    this.macroManager.Pause();
                    break;
                case "pause loop":
                    this.chatManager.PrintChat("Pausing at next /loop");
                    this.macroManager.Pause(true);
                    return;
                case "resume":
                    this.chatManager.PrintChat("Resuming");
                    this.macroManager.Resume();
                    break;
                case "stop":
                    this.chatManager.PrintChat("Stopping");
                    this.macroManager.Stop();
                    break;
                case "stop loop":
                    this.chatManager.PrintChat("Stopping at next /loop");
                    this.macroManager.Stop(true);
                    return;
                case "help":
                    this.helpWindow.Toggle();
                    break;
            }
        }
    }
}