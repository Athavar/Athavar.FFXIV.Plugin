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

    private const string RunMacroCommandName = "/runmacro";

    private readonly MacroConfigTab configTab;
    private readonly IDalamudServices dalamudServices;
    private readonly IChatManager chatManager;
    private readonly MacroManager macroManager;

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

        moduleManager.Register(this, this.Configuration.Enabled);
        PluginLog.LogDebug($"Module 'Macro' init. {this.Configuration}");

        this.dalamudServices.CommandManager.AddHandler(RunMacroCommandName, new CommandInfo(this.CommandHandler)
                                                                            {
                                                                                HelpMessage = "Start run a macro of the macro module.",
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
    public void Dispose() => this.dalamudServices.CommandManager.RemoveHandler(RunMacroCommandName);

    private void CommandHandler(string command, string arguments)
    {
        var args = arguments.Trim().Replace("\"", string.Empty);

        switch (this.macroManager.State)
        {
            case LoopState.Waiting:
            {
                var node = this.Configuration.GetAllNodes().FirstOrDefault(node => node.Name == args);

                if (node is MacroNode macro)
                {
                    this.macroManager.EnqueueMacro(macro);
                }
                else
                {
                    this.chatManager.PrintErrorMessage($"Fail to find macro with name: {args}");
                }

                break;
            }

            default:
                this.chatManager.PrintErrorMessage($"Fail to run macro: {this.macroManager.State}");
                break;
        }
    }
}