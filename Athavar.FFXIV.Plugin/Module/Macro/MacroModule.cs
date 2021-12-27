// <copyright file="MacroModule.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Module.Macro;

using System;
using System.Linq;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.Macro.Grammar.Commands;
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
        this.dalamudServices = provider.GetRequiredService<IDalamudServices>();
        this.chatManager = provider.GetRequiredService<IChatManager>();
        this.macroManager = provider.GetRequiredService<MacroManager>();
        this.configTab = provider.GetRequiredService<MacroConfigTab>();
        this.Configuration = provider.GetRequiredService<Configuration>().Macro!;

        this.pluginWindow = provider.GetRequiredService<PluginWindow>();
        this.helpWindow = provider.GetRequiredService<MacroHelpWindow>();

        moduleManager.Register(this, this.Configuration.Enabled);
        PluginLog.LogDebug($"Module 'Macro' init. {this.Configuration}");

        this.dalamudServices.CommandManager.AddHandler(MacroCommandName, new CommandInfo(this.OnChatCommand)
                                                                         {
                                                                             HelpMessage = "Commands of the macro module.",
                                                                             ShowInHelp = true,
                                                                         });
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
            var macroName = arguments[9..].Trim().Trim('"');
            var nodes = this.Configuration.GetAllNodes()
                            .OfType<MacroNode>()
                            .Where(node => node.Name.Trim() == macroName)
                            .ToArray();

            if (nodes.Length == 0)
            {
                this.chatManager.PrintErrorMessage("No macros match that name");
            }
            else if (nodes.Length > 1)
            {
                this.chatManager.PrintErrorMessage("More than one macro matches that name");
            }
            else
            {
                var node = nodes[0];
                this.chatManager.PrintInformationMessage($"Running macro \"{macroName}\"");
                this.macroManager.EnqueueMacro(node);
            }
        }
        else if (arguments.StartsWith("pause"))
        {
            this.chatManager.PrintInformationMessage("Pausing");
            this.macroManager.Pause();
        }
        else if (arguments.StartsWith("resume"))
        {
            this.chatManager.PrintInformationMessage("Resuming");
            this.macroManager.Resume();
        }
        else if (arguments.StartsWith("stop"))
        {
            this.chatManager.PrintInformationMessage("Stopping");
            this.macroManager.Clear();
        }
        else if (arguments.StartsWith("help"))
        {
            this.helpWindow.Toggle();
        }
    }
}