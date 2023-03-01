// <copyright file="PluginService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.UI;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
///     Plugin Service.
/// </summary>
internal class PluginService
{
    private readonly IDalamudServices dalamudServices;
    private readonly Configuration configuration;
    private readonly PluginWindow pluginWindow;

    private readonly WindowSystem windowSystem;
    private readonly IServiceProvider provider;

    private AutoTranslateWindow? translateWindow;

    /// <summary>
    ///     Initializes a new instance of the <see cref="PluginService" /> class.
    /// </summary>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="pluginWindow"><see cref="IPluginWindow" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="windowSystem"><see cref="WindowSystem" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    /// <param name="moduleManager"><see cref="IModuleManager" /> added by DI.</param>
    public PluginService(
        IDalamudServices dalamudServices,
        IPluginWindow pluginWindow,
        Configuration configuration,
        WindowSystem windowSystem,
        IServiceProvider provider,
        IModuleManager moduleManager)
    {
        this.dalamudServices = dalamudServices;
        this.pluginWindow = pluginWindow as PluginWindow ?? throw new InvalidOperationException();
        this.configuration = configuration;

        this.windowSystem = windowSystem;
        this.provider = provider;
    }

    /// <summary>
    ///     Start the plugin.
    /// </summary>
    public void Start()
    {
        // 1.
        PluginLog.LogDebug("Service Start");
        this.windowSystem.AddWindow(this.pluginWindow);

        this.dalamudServices.CommandManager.AddHandler(Plugin.CommandName, new CommandInfo(this.OnCommand)
        {
            HelpMessage =
                "Open the Configuration of Athavar's ToolsBox.",
        });
        this.dalamudServices.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.dalamudServices.PluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
        PluginLog.LogDebug("Service Started");
    }

    /// <summary>
    ///     Stop the plugin.
    /// </summary>
    public void Stop()
    {
        PluginLog.LogDebug("Service Stop");
        this.dalamudServices.PluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.dalamudServices.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
        this.dalamudServices.CommandManager.RemoveHandler(Plugin.CommandName);
        this.windowSystem.RemoveAllWindows();
        PluginLog.LogDebug("Service Stopped");
    }

    private void OnOpenConfigUi() => this.pluginWindow.Toggle();

    private void OnCommand(string command, string args)
    {
        if (string.IsNullOrEmpty(args))
        {
            this.pluginWindow.Toggle();
            return;
        }

        switch (args)
        {
            case "t":
            case "translate":
                this.translateWindow ??= this.provider.GetRequiredService<AutoTranslateWindow>();
                this.translateWindow.Toggle();
                break;
        }
    }
}