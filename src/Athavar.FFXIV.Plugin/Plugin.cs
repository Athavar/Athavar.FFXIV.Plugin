// <copyright file="Plugin.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using Athavar.FFXIV.Plugin.Cheat;
using Athavar.FFXIV.Plugin.Click;
using Athavar.FFXIV.Plugin.Common;
using Athavar.FFXIV.Plugin.Config;
using Athavar.FFXIV.Plugin.Data;
using Athavar.FFXIV.Plugin.Dps;
using Athavar.FFXIV.Plugin.DutyHistory;
using Athavar.FFXIV.Plugin.Macro;
using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.Models.Interfaces.Manager;
using Athavar.FFXIV.Plugin.OpcodeWizard;
using Athavar.FFXIV.Plugin.UI;
using Athavar.FFXIV.Plugin.Yes;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Events;

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

    private readonly ServiceProvider provider;
    private readonly PluginService servive;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="pluginInterface">Dalamud plugin interface.</param>
    public Plugin([RequiredVersion("1.0")] DalamudPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;

        this.provider = this.BuildProvider();
        this.servive = this.provider.GetRequiredService<PluginService>();
        this.servive.Start();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        this.servive.Stop();
        this.provider.DisposeAsync().GetAwaiter().GetResult();
    }

    private ServiceProvider BuildProvider()
    {
        return new ServiceCollection()
           .AddSingleton(
                o =>
                {
                    if (this.pluginInterface.ConfigFile.Exists)
                    {
                        Configuration.Migrate(this.pluginInterface);
                    }

                    return this.pluginInterface;
                })
           .AddSingleton<IPluginWindow, PluginWindow>()
           .AddSingleton<IModuleManager, ModuleManager>()
           .AddSingleton(_ => new WindowSystem("Athavar's Toolbox"))
           .AddCommon()
           .AddData()
           .AddModuleConfiguration()
           .AddClick()
           .AddMacroModule()
           .AddYesModule()
           .AddCheatModule()
           .AddDps()
           .AddOpcodeWizard()
           .AddDutyHistory()
#if DEBUG
#endif
           .AddSingleton<AutoTranslateWindow>()
           .AddSingleton<PluginService>()
           .AddSingleton<ILoggerProvider, HostLoggerProvider>()
           .BuildServiceProvider();
    }

    private sealed class HostLoggerProvider : ILoggerProvider
    {
        private readonly IPluginLogger pluginLog;

        public HostLoggerProvider(IPluginLogger pluginLog) => this.pluginLog = pluginLog;

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName) => new HostLogger(this.pluginLog);
    }

    private sealed class HostLogger : ILogger
    {
        private readonly IPluginLogger pluginLogger;

        public HostLogger(IPluginLogger pluginLogger) => this.pluginLogger = pluginLogger;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            var level = this.Convert(logLevel);
            this.pluginLogger.Write(level, exception, formatter(state, exception));
        }

        public bool IsEnabled(LogLevel logLevel) => this.pluginLogger.MinimumLogLevel <= this.Convert(logLevel);

        public IDisposable BeginScope<TState>(TState state) => NoopDisposable.Instance;

        private LogEventLevel Convert(LogLevel logLevel)
            => logLevel switch
            {
                LogLevel.Trace => LogEventLevel.Verbose,
                LogLevel.Debug => LogEventLevel.Debug,
                LogLevel.Information => LogEventLevel.Information,
                LogLevel.Warning => LogEventLevel.Warning,
                LogLevel.Error => LogEventLevel.Error,
                LogLevel.Critical => LogEventLevel.Fatal,
                LogLevel.None => LogEventLevel.Fatal,
                _ => LogEventLevel.Fatal,
            };

        private sealed class NoopDisposable : IDisposable
        {
            public static readonly NoopDisposable Instance = new();

            public void Dispose()
            {
            }
        }
    }
}