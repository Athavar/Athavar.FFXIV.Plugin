// <copyright file="IPluginLogger.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Config.Interfaces;

public interface IPluginLogger
{
    /// <summary>
    ///     Log a <see cref="F:Serilog.Events.LogEventLevel.Fatal"/> message to the Dalamud log for this plugin. This log level
    ///     should be
    ///     used primarily for unrecoverable errors or critical faults in a plugin.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    void Fatal(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Fatal(string,object[])"/>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    void Fatal(Exception? exception, string messageTemplate, params object[] values);

    /// <summary>
    ///     Log a <see cref="F:Serilog.Events.LogEventLevel.Error"/> message to the Dalamud log for this plugin. This log level
    ///     should be
    ///     used for recoverable errors or faults that impact plugin functionality.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    void Error(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Error(string,object[])"/>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    void Error(Exception? exception, string messageTemplate, params object[] values);

    /// <summary>
    ///     Log a <see cref="F:Serilog.Events.LogEventLevel.Warning"/> message to the Dalamud log for this plugin. This log
    ///     level should be
    ///     used for user error, potential problems, or high-importance messages that should be logged.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    void Warning(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Warning(string,object[])"/>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    void Warning(Exception? exception, string messageTemplate, params object[] values);

    /// <summary>
    ///     Log an <see cref="F:Serilog.Events.LogEventLevel.Information"/> message to the Dalamud log for this plugin. This
    ///     log level
    ///     should be used for general plugin operations and other relevant information to track a plugin's behavior.
    /// </summary>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    void Information(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Information(string,object[])"/>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    void Information(Exception? exception, string messageTemplate, params object[] values);

    /// <inheritdoc cref="Information(string,object[])"/>
    void Info(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Information(string,object[])"/>
    void Info(Exception? exception, string messageTemplate, params object[] values);

    /// <summary>
    ///     Log a <see cref="F:Serilog.Events.LogEventLevel.Debug"/> message to the Dalamud log for this plugin. This log level
    ///     should be
    ///     used for messages or information that aid with debugging or tracing a plugin's operations, but should not be
    ///     recorded unless requested.
    /// </summary>
    /// <remarks>
    ///     By default, this log level is below the default log level of Dalamud. Messages logged at this level will not be
    ///     recorded unless the global log level is specifically set to Debug or lower. If information should be generally
    ///     or easily accessible for support purposes without the user taking additional action, consider using the
    ///     Information level instead. Developers should <em>not</em> use this log level where it can be triggered on a
    ///     per-frame basis.
    /// </remarks>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    void Debug(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Debug(string,object[])"/>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    void Debug(Exception? exception, string messageTemplate, params object[] values);

    /// <summary>
    ///     Log a <see cref="F:Serilog.Events.LogEventLevel.Verbose"/> message to the Dalamud log for this plugin. This log
    ///     level is
    ///     intended almost primarily for development purposes and detailed tracing of a plugin's operations. Verbose logs
    ///     should not be used to expose information useful for support purposes.
    /// </summary>
    /// <remarks>
    ///     By default, this log level is below the default log level of Dalamud. Messages logged at this level will not be
    ///     recorded unless the global log level is specifically set to Verbose.
    /// </remarks>
    /// <param name="messageTemplate">Message template describing the event.</param>
    /// <param name="values">Objects positionally formatted into the message template.</param>
    void Verbose(string messageTemplate, params object[] values);

    /// <inheritdoc cref="Verbose(string,object[])"/>
    /// <param name="exception">An (optional) exception that should be recorded alongside this event.</param>
    void Verbose(Exception? exception, string messageTemplate, params object[] values);
}