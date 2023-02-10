// <copyright file="AthavarPluginException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Common.Exceptions;

using Dalamud.Logging;

public class AthavarPluginException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AthavarPluginException" /> class.
    /// </summary>
    public AthavarPluginException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AthavarPluginException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public AthavarPluginException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AthavarPluginException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">
    ///     The exception that is the cause of the current exception, or a null reference (
    ///     <see langword="Nothing" /> in Visual Basic) if no inner exception is specified.
    /// </param>
    public AthavarPluginException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Try to catch all exception.
    /// </summary>
    /// <param name="action">Action that can throw exception.</param>
    public static void CatchCrash(Action action)
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
}