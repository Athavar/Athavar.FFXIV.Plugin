// <copyright file="ChatManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager;

using System.Runtime.InteropServices;
using System.Threading.Channels;
using Athavar.FFXIV.Plugin.Common.Manager.Interface;
using Athavar.FFXIV.Plugin.Common.Utils;
using Athavar.FFXIV.Plugin.Config;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

/// <summary>
///     Manager that handles displaying output in the chat box.
/// </summary>
internal sealed class ChatManager : IDisposable, IChatManager
{
    private readonly Channel<SeString> chatBoxMessages = Channel.CreateUnbounded<SeString>();
    private readonly IDalamudServices dalamud;
    private readonly IFrameworkManager frameworkManager;

    [Signature("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9")]
    private readonly ProcessChatBoxDelegate processChatBox;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ChatManager"/> class.
    /// </summary>
    /// <param name="dalamud"><see cref="IDalamudServices"/> added by DI.</param>
    /// <param name="frameworkManager"><see cref="IFrameworkManager"/> added by DI.</param>
    /// <param name="addressResolver"><see cref="AddressResolver"/> added by DI.</param>
    public ChatManager(IDalamudServices dalamud, IFrameworkManager frameworkManager, AddressResolver addressResolver)
    {
        this.dalamud = dalamud;
        this.frameworkManager = frameworkManager;

        // init processChatBox
        this.processChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(addressResolver.ProcessChatBox);

        this.frameworkManager.Subscribe(this.FrameworkUpdate);
#if DEBUG
        // this.dalamud.ChatGui.ChatMessage += this.OnChatMessageDebug;
#endif
    }

    private unsafe delegate void ProcessChatBoxDelegate(UIModule* uiModule, nint message, nint unused, byte a4);

    private unsafe delegate void ProcessChat(Utf8String* message, int flag, nint f);

    /// <inheritdoc/>
    public void Dispose()
    {
#if DEBUG
        // this.dalamud.ChatGui.ChatMessage -= this.OnChatMessageDebug;
#endif
        this.frameworkManager.Unsubscribe(this.FrameworkUpdate);
        this.chatBoxMessages.Writer.Complete();
    }

    /// <inheritdoc/>
    public void PrintChat(SeString message, XivChatType? type = null) => this.SendMessageInternal(message, type);

    /// <inheritdoc/>
    public void PrintChat(SeString message, IChatManager.UiColor color, XivChatType? type = null) => this.SendMessageInternal(message, type, color);

    /// <inheritdoc/>
    public void PrintErrorMessage(SeString message)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Constants.PluginName}]"));
        this.dalamud.ChatGui.PrintError(message);
    }

    /// <inheritdoc/>
    public async void SendMessage(string message) => await this.chatBoxMessages.Writer.WriteAsync(SeStringHelper.Parse(message));

    /// <inheritdoc/>
    public async void SendMessage(SeString message) => await this.chatBoxMessages.Writer.WriteAsync(message);

    /// <inheritdoc/>
    public void Clear()
    {
        var reader = this.chatBoxMessages.Reader;
        while (reader.Count > 0 && reader.TryRead(out var _))
        {
        }
    }

    private void SendMessageInternal(SeString message, XivChatType? type, IChatManager.UiColor? color = null)
    {
        message.Payloads.Insert(0, new TextPayload($"[{Constants.PluginName}] "));
        if (color is not null)
        {
            message.Payloads.Insert(0, new UIForegroundPayload((ushort)color));
            _ = message.Payloads.Append(UIForegroundPayload.UIForegroundOff);
        }

        if (type is not null)
        {
            this.dalamud.ChatGui.Print(new XivChatEntry
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

    private void FrameworkUpdate(IFramework framework)
    {
        if (this.chatBoxMessages.Reader.Count == 0)
        {
            return;
        }

        if (this.chatBoxMessages.Reader.TryRead(out var message))
        {
            this.SendMessageInternal(message);
        }
    }

    private void SendMessageInternal(SeString message)
    {
        var bytes = message.Encode();
        switch (bytes.Length)
        {
            case 0:
                return;
            case > 500:
                this.PrintErrorMessage($"Message exceeds byte size of 500({bytes.Length})");
                return;
        }

        var payloadMessage = message.ToString();
        if (payloadMessage.Length != this.dalamud.PluginInterface.Sanitizer.Sanitize(payloadMessage).Length)
        {
            this.PrintErrorMessage("Message contained invalid characters");
            return;
        }

        this.SendMessageUnsafe(bytes);
    }

    /// <summary>
    ///     <para>
    ///         Send a given message to the chat box. <b>This can send chat to the server.</b>
    ///     </para>
    ///     <para>
    ///         <b>This method is unsafe.</b> This method does no checking on your input and
    ///         may send content to the server that the normal client could not. You must
    ///         verify what you're sending and handle content and length to properly use
    ///         this.
    ///     </para>
    /// </summary>
    /// <param name="message">Message to send.</param>
    /// <exception cref="InvalidOperationException">If the signature for this function could not be found.</exception>
    private unsafe void SendMessageUnsafe(byte[] message)
    {
        if (this.processChatBox == null)
        {
            throw new InvalidOperationException("Could not find signature for chat sending");
        }

        var uiModule = Framework.Instance()->GetUiModule();
        using var payload = new ChatPayload(message);
        var chatPayloadPtr = Marshal.AllocHGlobal(400);
        Marshal.StructureToPtr(payload, chatPayloadPtr, false);
        this.processChatBox(uiModule, chatPayloadPtr, nint.Zero, 0);
        Marshal.FreeHGlobal(chatPayloadPtr);
    }

#if DEBUG
    private void OnChatMessageDebug(XivChatType type, uint senderId, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        var sender2 = sender;
        var message2 = message;
        /*
        AthavarPluginException.CatchCrash(() => { PluginLog.Debug($"SenderId={senderId},Sender={sender2.TextValue},Bytes={message2.Encode().Length},MessagePayloads={string.Join(';', message2.Payloads.Select(p => p.ToString()))}"); });
         */
    }
#endif

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct ChatPayload : IDisposable
    {
        [FieldOffset(0)]
        private readonly nint textPtr;

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