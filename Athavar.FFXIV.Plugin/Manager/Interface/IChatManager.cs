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
    ///     Print a information message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    void PrintInformationMessage(string message);

    /// <summary>
    ///     Print a information message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    void PrintInformationMessage(SeString message);

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
}