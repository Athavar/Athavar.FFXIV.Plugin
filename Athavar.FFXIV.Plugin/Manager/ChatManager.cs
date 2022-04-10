// <copyright file="ChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>

namespace Athavar.FFXIV.Plugin.Manager;

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using Athavar.FFXIV.Plugin.Manager.Interface;
using Athavar.FFXIV.Plugin.Utils;
using Dalamud.Game;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Logging;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Manager that handles displaying output in the chat box.
/// </summary>
internal class ChatManager : IDisposable, IChatManager
{
    private readonly Channel<SeString> chatBoxMessages = Channel.CreateUnbounded<SeString>();
    private readonly IDalamudServices dalamud;

    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9")]
    private readonly ProcessChatBoxDelegate processChatBox = null!;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatManager" /> class.
    /// </summary>
    /// <param name="dalamud"><see cref="IDalamudServices" /> added by DI.</param>
    public ChatManager(IDalamudServices dalamud)
    {
        this.dalamud = dalamud;

        // init processChatBox
        SignatureHelper.Initialise(this);

        this.dalamud.Framework.Update += this.FrameworkUpdate;
#if DEBUG
        // this.dalamud.ChatGui.ChatMessage += this.OnChatMessageDebug;
#endif
    }

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, IntPtr message, IntPtr unused, byte a4);

    /// <inheritdoc />
    public void Dispose()
    {
#if DEBUG
        // this.dalamud.ChatGui.ChatMessage -= this.OnChatMessageDebug;
#endif
        this.dalamud.Framework.Update -= this.FrameworkUpdate;
        this.chatBoxMessages.Writer.Complete();
    }

    /// <inheritdoc />
    public void PrintChat(string message, XivChatType? type = null) => this.PrintChat((SeString)message, type);

    /// <inheritdoc />
    public void PrintChat(SeString message, XivChatType? type = null)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Plugin.PluginName}] "));
        if (type is not null)
        {
            this.dalamud.ChatGui.PrintChat(new XivChatEntry
                                           {
                                               Message = message,
                                               Type = type.Value,
                                           });
        }
        else
        {
            this.dalamud.ChatGui.Print(message);
        }
    }

    /// <inheritdoc />
    public void PrintErrorMessage(string message) => this.dalamud.ChatGui.PrintError($"[{Plugin.PluginName}] {message}");

    /// <inheritdoc />
    public void PrintErrorMessage(SeString message)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Plugin.PluginName}] "));
        this.dalamud.ChatGui.PrintError(message);
    }

    /// <inheritdoc />
    public async void SendMessage(string message) => await this.chatBoxMessages.Writer.WriteAsync(SeStringHelper.Parse(message));

    /// <inheritdoc />
    public async void SendMessage(SeString message) => await this.chatBoxMessages.Writer.WriteAsync(message);

    /// <inheritdoc />
    public void Clear()
    {
        var reader = this.chatBoxMessages.Reader;
        while (reader.Count > 0 && reader.TryRead(out var _))
        {
        }
    }

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

        using var payload = new ChatPayload(Encoding.UTF8.GetBytes(message));
        var payloadPtr = Marshal.AllocHGlobal(400);
        Marshal.StructureToPtr(payload, payloadPtr, false);

        this.processChatBox(uiModule, payloadPtr, IntPtr.Zero, 0);

        Marshal.FreeHGlobal(payloadPtr);
    }

    private unsafe void SendMessageInternal(SeString message)
    {
        var messagePayload = message.Encode();
        if (messagePayload.Length > 500)
        {
            this.PrintErrorMessage($"Message exceeds byte size of 500({messagePayload.Length})");
            return;
        }

        var framework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        var uiModule = framework->GetUiModule();

        using var chatPayload = new ChatPayload(messagePayload);
        var chatPayloadPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ChatPayload>());
        Marshal.StructureToPtr(chatPayload, chatPayloadPtr, false);

        this.processChatBox(uiModule, chatPayloadPtr, IntPtr.Zero, 0);

        Marshal.FreeHGlobal(chatPayloadPtr);
    }

#if DEBUG
    private void OnChatMessageDebug(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var sender2 = sender;
        var message2 = message;
        Plugin.CatchCrash(() => { PluginLog.Debug($"SenderId={senderId},Sender={sender2.TextValue},Bytes={message2.Encode().Length},MessagePayloads={string.Join(';', message2.Payloads.Select(p => p.ToString()))}"); });
    }
#endif

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

        internal ChatPayload(byte[] payload)
        {
            this.textPtr = Marshal.AllocHGlobal(payload.Length + 30);

            Marshal.Copy(payload, 0, this.textPtr, payload.Length);
            Marshal.WriteByte(this.textPtr + payload.Length, 0);

            this.textLen = (ulong)(payload.Length + 1);

            this.unk1 = 64;
            this.unk2 = 0;
        }

        public void Dispose() => Marshal.FreeHGlobal(this.textPtr);
    }
}