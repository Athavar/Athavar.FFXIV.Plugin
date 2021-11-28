// <copyright file="IChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager.Interface;

using Dalamud.Game.Text.SeStringHandling;

/// <summary>
///     Print messages to the in-game chat.
/// </summary>
internal interface IChatManager
{
    /// <summary>
    ///     Print a normal message.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void PrintMessage(string message);

    /// <summary>
    ///     Print a message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void PrintMessage(SeString message);

    /// <summary>
    ///     Print an error message.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void PrintError(string message);

    /// <summary>
    ///     Print an error message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void PrintError(SeString message);

    /// <summary>
    ///     Process a command through the chat box.
    /// </summary>
    /// <param name="message">Message to send.</param>
    void SendMessage(string message);
}