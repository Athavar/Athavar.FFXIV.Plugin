// <copyright file="Plugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Threading;
using System.Threading.Tasks;
using Athavar.FFXIV.Plugin.Lib.ClickLib;
using Athavar.FFXIV.Plugin.Manager;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Module;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
///     Main plugin implementation.
/// </summary>
public sealed class Plugin : IDalamudPlugin
{
    /// <summary>
    ///     prefix of the command.
    /// </summary>
    internal const string CommandName = "/ath";

    /// <summary>
    ///     The Plugin name.
    /// </summary>
    internal const string PluginName = "Athavar's Toolbox";

    private readonly DalamudPluginInterface pluginInterface;

    private readonly CancellationTokenSource tokenSource;

    private IHostLifetime? hostLifetime;

    private IHost? host;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Plugin" /> class.
    /// </summary>
    /// <param name="pluginInterface">Dalamud plugin interface.</param>
    public Plugin(
        [RequiredVersion("1.0")]
        DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        this.tokenSource = new CancellationTokenSource();

        _ = Task.Run(
            async () =>
            {
                Resolver.Initialize();

                this.host = Host.CreateDefaultBuilder().ConfigureLogging(this.ConfigureLogging)
                   .ConfigureServices(this.ConfigureServices)
                   .Build();

                this.hostLifetime = this.host.Services.GetRequiredService<IHostLifetime>();

                try
                {
                    await this.host.StartAsync(this.tokenSource.Token);
                }
                catch (Exception ex)
                {
                    PluginLog.Error(ex, "Exception occured.");
                }
            },
            this.tokenSource.Token);
    }

    /// <inheritdoc />
    public string Name => PluginName;

    /// <inheritdoc />
    public void Dispose()
    {
        this.tokenSource.Cancel();
        if (this.hostLifetime is not null)
        {
            var task = this.hostLifetime.StopAsync(CancellationToken.None);
            task.Wait();
        }

        if (this.host is not null)
        {
            var task = this.host.StopAsync(CancellationToken.None);
            task.Wait();
        }

        this.host?.Dispose();
        this.tokenSource.Dispose();
    }

    /// <summary>
    ///     Try to catch all exception.
    /// </summary>
    /// <param name="action">Action that can throw exception.</param>
    internal static void CatchCrash(Action action)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception ex)
        {
            PluginLog.Error(ex, "Don't crash the game");
        }
    }

    private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder builder)
    {
        builder.AddDalamudLogger();
        builder.AddDebug();
        builder.Configure(options =>
        {
            options.ActivityTrackingOptions =
                ActivityTrackingOptions.SpanId |
                ActivityTrackingOptions.TraceId |
                ActivityTrackingOptions.ParentId;
        });
        builder.SetMinimumLevel(LogLevel.Debug);
    }

    private void ConfigureServices(HostBuilderContext context, IServiceCollection service)
    {
        service.AddSingleton(this.pluginInterface);
        service.AddSingleton<IDalamudServices, DalamudServices>();
        service.AddSingleton<IModuleManager, ModuleManager>();
        service.AddSingleton<ILocalizerManager, LocalizerManager>();
        service.AddSingleton<PluginWindow>();
        service.AddSingleton(o =>
        {
            var pi = o.GetRequiredService<IDalamudServices>().PluginInterface;
            var c = (Configuration?)pi.GetPluginConfig() ?? new Configuration();
            c.Setup(this.pluginInterface);
            return c;
        });
        service.AddSingleton(_ => new WindowSystem("Athavar's Toolbox"));

        service.AddSingleton<IChatManager, ChatManager>();
        service.AddSingleton<EquipmentScanner>();
        service.AddSingleton<KeyStateExtended>();
        service.AddSingleton<AutoTranslateWindow>();
        service.AddSingleton<IClick, Click>();
        service.AddSingleton<ICommandInterface, CommandInterface>();

        service.AddMacroModule();
        service.AddYesModule();
        service.AddInstancinatorModule();
        service.AddAutoSpearModule();
        service.AddCheatModule();
#if DEBUG
        service.AddHuntLinkModule();
        service.AddItemInspectorModule();
#endif

        service.AddHostedService<PluginService>();
    }
}