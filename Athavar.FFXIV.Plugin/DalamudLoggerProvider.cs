// <copyright file="DalamudLoggerProvider.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin;

using System;
using System.Collections.Concurrent;
using Dalamud.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

/// <summary>
///     Dalamud Logger Provider.
/// </summary>
internal class DalamudLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, DalamudLogger> loggers = new();

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName) => this.loggers.GetOrAdd(categoryName, name => new DalamudLogger(name));

    /// <inheritdoc />
    public void Dispose() => this.loggers.Clear();
}

/// <summary>
///     Dalamud Logger.
/// </summary>
internal class DalamudLogger : ILogger
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DalamudLogger" /> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public DalamudLogger(string name)
    {
    }

    /// <inheritdoc />
    public IDisposable BeginScope<TState>(TState state) => new DummyDispose();

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel) => true;

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        switch (logLevel)
        {
            case LogLevel.Trace:
            case LogLevel.Debug:
                if (exception is null)
                {
                    PluginLog.LogDebug(formatter(state, exception));
                }
                else
                {
                    PluginLog.LogDebug(exception, formatter(state, exception));
                }

                break;

            case LogLevel.Information:
                if (exception is null)
                {
                    PluginLog.LogInformation(formatter(state, exception));
                }
                else
                {
                    PluginLog.LogInformation(exception, formatter(state, exception));
                }

                break;
            case LogLevel.Warning:
                if (exception is null)
                {
                    PluginLog.LogWarning(formatter(state, exception));
                }
                else
                {
                    PluginLog.LogWarning(exception, formatter(state, exception));
                }

                break;
            case LogLevel.Error:
                if (exception is null)
                {
                    PluginLog.LogError(formatter(state, exception));
                }
                else
                {
                    PluginLog.LogError(exception, formatter(state, exception));
                }

                break;
            case LogLevel.Critical:
                if (exception is null)
                {
                    PluginLog.LogFatal(formatter(state, exception));
                }
                else
                {
                    PluginLog.LogFatal(exception, formatter(state, exception));
                }

                break;
            case LogLevel.None:
                break;
        }
    }

    private class DummyDispose : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

/// <summary>
///     Allow add <see cref="DalamudLoggerProvider" /> to <see cref="ILoggingBuilder" />.
/// </summary>
public static class DalamudLoggerExtensions
{
    /// <summary>
    ///     Add <see cref="DalamudLoggerProvider" /> to <see cref="ILoggingBuilder" />.
    /// </summary>
    /// <param name="builder">The <see cref="ILoggingBuilder" />.</param>
    /// <returns>The <see cref="ILoggingBuilder" /> for chaining.</returns>
    public static ILoggingBuilder AddDalamudLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(
            ServiceDescriptor.Singleton<ILoggerProvider, DalamudLoggerProvider>());

        return builder;
    }
}