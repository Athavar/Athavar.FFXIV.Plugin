// <copyright file="PluginService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.AutoSpear;
using Athavar.FFXIV.Plugin.Module.Cheat;
using Athavar.FFXIV.Plugin.Module.CraftQueue;
using Athavar.FFXIV.Plugin.Module.Instancinator;
using Athavar.FFXIV.Plugin.Module.ItemInspector;
using Athavar.FFXIV.Plugin.Module.Macro;
using Athavar.FFXIV.Plugin.Module.Yes;
using Athavar.FFXIV.Plugin.Utils;
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
    /// <param name="logger"><see cref="ILogger{TCategoryName}" /> added by DI.</param>
    /// <param name="appLifetime"><see cref="IHostApplicationLifetime" /> added by DI.</param>
    /// <param name="dalamudServices"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="pluginWindow"><see cref="PluginWindow" /> added by DI.</param>
    /// <param name="configuration"><see cref="Configuration" /> added by DI.</param>
    /// <param name="windowSystem"><see cref="WindowSystem" /> added by DI.</param>
    /// <param name="provider"><see cref="IServiceProvider" /> added by DI.</param>
    public PluginService(
        IDalamudServices dalamudServices,
        PluginWindow pluginWindow,
        Configuration configuration,
        WindowSystem windowSystem,
        IServiceProvider provider)
    {
        this.dalamudServices = dalamudServices;
        this.pluginWindow = pluginWindow;
        this.configuration = configuration;

        this.windowSystem = windowSystem;
        this.provider = provider;

        _ = provider.GetRequiredService<MacroModule>();
        _ = provider.GetRequiredService<YesModule>();
        _ = provider.GetRequiredService<InstancinatorModule>();
        _ = provider.GetRequiredService<AutoSpearModule>();
        _ = provider.GetRequiredService<CheatModule>();
        _ = provider.GetRequiredService<CraftQueueModule>();
#if DEBUG
        _ = provider.GetRequiredService<ItemInspectorModule>();
#endif
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