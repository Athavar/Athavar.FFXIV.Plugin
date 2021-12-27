// <copyright file="PluginService.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Manager;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module.HuntLink;
using Athavar.FFXIV.Plugin.Module.Macro;
using Athavar.FFXIV.Plugin.Module.Yes;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
///     Plugin Service.
/// </summary>
internal class PluginService : IHostedService
{
    private readonly IDalamudServices dalamudServices;
    private readonly Configuration configuration;
    private readonly ILogger logger;
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
        ILogger<PluginService> logger,
        IHostApplicationLifetime appLifetime,
        IDalamudServices dalamudServices,
        PluginWindow pluginWindow,
        Configuration configuration,
        WindowSystem windowSystem,
        IServiceProvider provider)
    {
        this.logger = logger;
        this.dalamudServices = dalamudServices;
        this.pluginWindow = pluginWindow;
        this.configuration = configuration;

        appLifetime.ApplicationStarted.Register(this.OnStarted);
        appLifetime.ApplicationStopping.Register(this.OnStopping);
        appLifetime.ApplicationStopped.Register(this.OnStopped);

        this.windowSystem = windowSystem;
        this.provider = provider;

        var address = provider.GetRequiredService<PluginAddressResolver>();
        address.Setup(this.dalamudServices.SigScanner);
        _ = provider.GetRequiredService<MacroModule>();
        _ = provider.GetRequiredService<YesModule>();
#if DEBUG
        _ = provider.GetRequiredService<HuntLinkModule>();
#endif
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        // 1.
        this.logger.LogDebug("Service Start");
        this.windowSystem.AddWindow(this.pluginWindow);

        this.dalamudServices.CommandManager.AddHandler(Plugin.CommandName, new CommandInfo(this.OnCommand)
                                                                           {
                                                                               HelpMessage =
                                                                                   "Open the Configuration of Athavar's ToolsBox.",
                                                                           });

        var dal = this.dalamudServices as DalamudServices;

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        // 4.
        this.logger.LogDebug("Service Stop");

        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        // 2.
        this.logger.LogDebug("Service Started");
        this.dalamudServices.PluginInterface.UiBuilder.Draw += this.windowSystem.Draw;
        this.dalamudServices.PluginInterface.UiBuilder.OpenConfigUi += this.OnOpenConfigUi;
    }

    private void OnStopping()
    {
        // 3.
        this.logger.LogDebug("Service Stopping");

        this.dalamudServices.CommandManager.RemoveHandler(Plugin.CommandName);
        this.dalamudServices.PluginInterface.UiBuilder.OpenConfigUi -= this.OnOpenConfigUi;
        this.dalamudServices.PluginInterface.UiBuilder.Draw -= this.windowSystem.Draw;
    }

    private void OnStopped()
    {
        // 5.
        this.logger.LogDebug("Service Stopped");

        // remove all remaining windows.
        this.windowSystem.RemoveAllWindows();
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