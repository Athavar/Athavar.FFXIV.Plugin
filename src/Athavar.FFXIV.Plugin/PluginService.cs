// <copyright file="PluginService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Athavar.FFXIV.Plugin.UI;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;

/// <summary>
///     Plugin Service.
/// </summary>
internal sealed class PluginService
{
    private readonly IDalamudServices dalamudServices;
    private readonly IPluginLogger logger;
    private readonly PluginWindow pluginWindow;

    private readonly WindowSystem windowSystem;
    private readonly IServiceProvider provider;
    private readonly IModuleManager moduleManager;

    private AutoTranslateWindow? translateWindow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginService"/> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="pluginWindow"><see cref="IPluginWindow"/> added by DI.</param>
    /// <param name="windowSystem"><see cref="WindowSystem"/> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider"/> added by DI.</param>
    /// <param name="moduleManager"><see cref="IModuleManager"/> added by DI.</param>
    public PluginService(
        IDalamudServices dalamudServices,
        IPluginWindow pluginWindow,
        WindowSystem windowSystem,
        IServiceProvider provider,
        IModuleManager moduleManager)
    {
        this.dalamudServices = dalamudServices;
        this.logger = this.dalamudServices.PluginLogger;
        this.pluginWindow = pluginWindow as PluginWindow ?? throw new InvalidOperationException();

        this.windowSystem = windowSystem;
        this.provider = provider;
        this.moduleManager = moduleManager;
    }

    /// <summary>
    ///     Start the plugin.
    /// </summary>
    public void Start()
    {
        // 1.
        this.logger.Debug("Service Start");
        this.windowSystem.AddWindow(this.pluginWindow);

        this.logger.Debug("Load Modules");
        this.moduleManager.LoadModules();

        this.dalamudServices.CommandManager.AddHandler(Plugin.CommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage =
                "Open the Configuration of Athavar's ToolsBox.",
        });
        this.dalamudServices.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.dalamudServices.PluginInterface.UiBuilder.OpenMainUi += this.OnOpenConfigUi;
        this.logger.Debug("Service Started");
    }

    /// <summary>
    ///     Stop the plugin.
    /// </summary>
    public void Stop()
    {
        this.logger.Debug("Service Stop");
        this.dalamudServices.PluginInterface.UiBuilder.OpenMainUi -= this.OnOpenConfigUi;
        this.dalamudServices.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.dalamudServices.CommandManager.RemoveHandler(Plugin.CommandName);
        this.windowSystem.RemoveAllWindows();
        this.logger.Debug("Service Stopped");
    }

    private void OnOpenConfigUi() => this.pluginWindow.Toggle();

    private void OnCommand(string command, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            this.pluginWindow.Toggle();
            return;
        }

        /* Disable, currently broken
        switch (args)
        {
            case "t":
            case "translate":
                this.translateWindow ??= this.provider.GetRequiredService<AutoTranslateWindow>();
                this.translateWindow.Toggle();
                break;
        }*/
    }
}