// <copyright file="AthavarPluginException.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin;

using System;

internal class AthavarPluginException : Exception
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
    /// <param name="message">The exception message.</param>
    public AthavarPluginException(string message)
        : base(message)
    {
    }
}