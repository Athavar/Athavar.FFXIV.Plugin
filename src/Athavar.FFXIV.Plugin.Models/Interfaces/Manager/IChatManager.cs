// <copyright file="IChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Models.Interfaces;

using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;

/// <summary>
///     Print messages to the in-game chat.
/// </summary>
public interface IChatManager
{
    /// <summary>
    ///     Colors for SeString payloads.
    /// </summary>
    /// <remarks>
    ///     https://github.com/Caraxi/SimpleTweaksPlugin/blob/main/images/placeholderHelp.png.
    /// </remarks>
    public enum UiColor
    {
        /// <summary>
        ///     Orange.
        /// </summary>
        Orange = 500,

        /// <summary>
        ///     Blue.
        /// </summary>
        Blue = 502,

        /// <summary>
        ///     Green.
        /// </summary>
        Green = 504,

        /// <summary>
        ///     Yellow.
        /// </summary>
        Yellow = 506,

        /// <summary>
        ///     Red.
        /// </summary>
        Red = 508,
    }

    /// <summary>
    ///     Print a information message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    /// <param name="type">The chat type.</param>
    void PrintChat(SeString message, XivChatType? type = null);

    /// <summary>
    ///     Print a information message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    /// <param name="color">The text color.</param>
    /// <param name="type">The chat type.</param>
    void PrintChat(SeString message, UiColor color, XivChatType? type = null);

    /// <summary>
    ///     Print an error message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void PrintErrorMessage(SeString message);

    /// <summary>
    ///     Process a command through the chat box.
    ///     Can also modify the message.
    /// </summary>
    /// <param name="message">Message to send.</param>
    void SendMessage(string message);

    void SendMessage(SeString message);

    /// <summary>
    ///     Clear the queue of messages to send to the chat box.
    /// </summary>
    public void Clear();
}