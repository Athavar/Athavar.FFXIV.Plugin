// <copyright file="IChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager.Interface;

using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

/// <summary>
///     Print messages to the in-game chat.
/// </summary>
internal interface IChatManager
{
    /// <summary>
    ///     Print a information message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    /// <param name="type">The chat type.</param>
    void PrintChat(string message, XivChatType? type = null);

    /// <summary>
    ///     Print a information message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    /// <param name="type">The chat type.</param>
    void PrintChat(SeString message, XivChatType? type = null);

    /// <summary>
    ///     Print an error message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void PrintErrorMessage(string message);

    /// <summary>
    ///     Print an error message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void PrintErrorMessage(SeString message);

    /// <summary>
    ///     Process a command through the chat box.
    /// </summary>
    /// <param name="message">Message to send.</param>
    void SendMessage(string message);

    void SendMessage(SeString message);

    /// <summary>
    ///     Clear the queue of messages to send to the chat box.
    /// </summary>
    public void Clear();
}