// <copyright file="BaseLogger.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Athavar.FFXIV.Plugin.Config.Interfaces;
using Serilog.Events;

public abstract class BaseLogger : IPluginLogger
{
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

    /// <summary>
    ///     Write a raw log event to the plugin's log. Used for interoperability with other log systems, as well as
    ///     advanced use cases.
    /// </summary>
    /// <param name="level">The log level for this event.</param>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    protected abstract void Write(
        LogEventLevel level,
        Exception? exception,
        string messageTemplate,
        params object[] values);
}