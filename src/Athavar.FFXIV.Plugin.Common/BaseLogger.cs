// <copyright file="BaseLogger.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Serilog.Events;

public abstract class BaseLogger : IPluginLogger
{
    /// <inheritdoc/>
    public abstract LogEventLevel MinimumLogLevel { get; }

    /// <inheritdoc/>
    public void Fatal(string messageTemplate, params object[] values) => this.Fatal(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Fatal(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Fatal, exception, messageTemplate, values);

    /// <inheritdoc/>
    public void Error(string messageTemplate, params object[] values) => this.Error(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Error(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Error, exception, messageTemplate, values);

    /// <inheritdoc/>
    public void Warning(string messageTemplate, params object[] values) => this.Warning(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Warning(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Warning, exception, messageTemplate, values);

    /// <inheritdoc/>
    public void Information(string messageTemplate, params object[] values) => this.Information(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Information(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Information, exception, messageTemplate, values);

    /// <inheritdoc/>
    public void Info(string messageTemplate, params object[] values) => this.Info(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Info(Exception? exception, string messageTemplate, params object[] values) => this.Information(exception, messageTemplate, values);

    /// <inheritdoc/>
    public void Debug(string messageTemplate, params object[] values) => this.Debug(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Debug(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Debug, exception, messageTemplate, values);

    /// <inheritdoc/>
    public void Verbose(string messageTemplate, params object[] values) => this.Verbose(null, messageTemplate, values);

    /// <inheritdoc/>
    public void Verbose(Exception? exception, string messageTemplate, params object[] values) => this.Write(LogEventLevel.Verbose, exception, messageTemplate, values);

    /// <inheritdoc/>
    public abstract void Write(
        LogEventLevel level,
        Exception? exception,
        string messageTemplate,
        params object[] values);
}