// <copyright file="Logger.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Dalamud.Plugin.Services;
using Serilog.Events;

internal sealed class Logger : BaseLogger
{
    private readonly IPluginLog pluginLog;

    /// <summary>
    ///     Initializes a new instance of the <see cref="Logger"/> class.
    /// </summary>
    /// <param name="pluginLog"><see cref="IPluginLog"/> as logging target.</param>
    public Logger(IPluginLog pluginLog) => this.pluginLog = pluginLog;

    /// <inheritdoc/>
    protected override void Write(LogEventLevel level, Exception? exception, string messageTemplate, params object[] values) => this.pluginLog.Write(level, exception, messageTemplate, values);
}