// <copyright file="ChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager;

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Dalamud.Game;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Manager that handles displaying output in the chat box.
/// </summary>
internal class ChatManager : IDisposable, IChatManager
{
    private readonly Channel<string> chatBoxMessages = Channel.CreateUnbounded<string>();
    private readonly IDalamudServices dalamud;
    private readonly ProcessChatBoxDelegate processChatBox;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatManager" /> class.
    /// </summary>
    /// <param name="dalamud"><see cref="IDalamudServices" /> added by DI.</param>
    /// <param name="addressResolver"><see cref="PluginAddressResolver" /> added by DI.</param>
    public ChatManager(IDalamudServices dalamud, PluginAddressResolver addressResolver)
    {
        this.dalamud = dalamud;

        this.processChatBox =
            Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(addressResolver.SendChatAddress);

        this.dalamud.Framework.Update += this.FrameworkUpdate;
    }

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);

    /// <inheritdoc />
    public void Dispose()
    {
        this.dalamud.Framework.Update -= this.FrameworkUpdate;
        this.chatBoxMessages.Writer.Complete();
    }

    /// <summary>
    ///     Print a message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    public void PrintMessage(string message) => this.dalamud.ChatGui.Print($"[{Plugin.PluginName}] {message}");

    /// <summary>
    ///     Print a message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void PrintMessage(SeString message)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Plugin.PluginName}] "));
        this.dalamud.ChatGui.Print(message);
    }

    /// <summary>
    ///     Print an error message to the chat window.
    /// </summary>
    /// <param name="message">The message to print.</param>
    public void PrintError(string message) => this.dalamud.ChatGui.PrintError($"[{Plugin.PluginName}] {message}");

    /// <summary>
    ///     Print an error message to the chat window.
    /// </summary>
    /// <param name="message">Message to display.</param>
    public void PrintError(SeString message)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Plugin.PluginName}] "));
        this.dalamud.ChatGui.PrintError(message);
    }

    /// <summary>
    ///     Process a command through the chat box.
    /// </summary>
    /// <param name="message">Message to send.</param>
    public async void SendMessage(string message) => await this.chatBoxMessages.Writer.WriteAsync(message);

    private void FrameworkUpdate(Framework framework)
    {
        if (this.chatBoxMessages.Reader.TryRead(out var message))
        {
            this.SendMessageInternal(message);
        }
    }

    private unsafe void SendMessageInternal(string message)
    {
        var framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        var uiModule = framework->GetUiModule();

        using var payload = new ChatPayload(message);
        var payloadPtr = Marshal.AllocHGlobal(400);
        Marshal.StructureToPtr(payload, payloadPtr, false);

        this.processChatBox(uiModule, payloadPtr, IntPtr.Zero, 0);

        Marshal.FreeHGlobal(payloadPtr);
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct ChatPayload : IDisposable
    {
        [FieldOffset(0)]
        private readonly IntPtr textPtr;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(16)]
        private readonly ulong textLen;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(8)]
        private readonly ulong unk1;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        [FieldOffset(24)]
        private readonly ulong unk2;

        internal ChatPayload(string text)
        {
            var stringBytes = Encoding.UTF8.GetBytes(text);
            this.textPtr = Marshal.AllocHGlobal(stringBytes.Length + 30);

            Marshal.Copy(stringBytes, 0, this.textPtr, stringBytes.Length);
            Marshal.WriteByte(this.textPtr + stringBytes.Length, 0);

            this.textLen = (ulong)(stringBytes.Length + 1);

            this.unk1 = 64;
            this.unk2 = 0;
        }

        public void Dispose() => Marshal.FreeHGlobal(this.textPtr);
    }
}