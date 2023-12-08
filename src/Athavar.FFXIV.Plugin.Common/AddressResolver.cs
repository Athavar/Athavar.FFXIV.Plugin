// <copyright file="AddressResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common;

using Dalamud.Game;

public sealed class AddressResolver : BaseAddressResolver
{
    /// <summary>
    ///     Gets or sets the pointer to the method responsible for handling CfPop packets.
    /// </summary>
    public nint CfPopPacketHandler { get; set; }

    /// <summary>
    ///     Gets or sets the pointer to the method responsible for processing chatBox inputs.
    /// </summary>
    public nint ProcessChatBox { get; set; }

    /// <summary>
    ///     Gets or sets the pointer to the method responsible for receiving agent events.
    /// </summary>
    public nint AgentReceiveEvent { get; set; }

    /// <summary>
    ///     Gets or sets the pointer to the method responsible for receiving ActorControl network packages.
    /// </summary>
    public nint ActorControlHandler { get; set; }

    protected override void Setup64Bit(ISigScanner scanner)
    {
        this.CfPopPacketHandler = scanner.ScanText("40 53 57 48 83 EC 78 48 8B D9 48 8D 0D");
        this.ProcessChatBox = scanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B FA 48 8B D9 45 84 C9");
        this.AgentReceiveEvent = scanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B DA 4D 8B D0 32 D2");

        // from: https://github.com/Kouzukii/ffxiv-deathrecap/blob/1d596906358d7f170c3d40ed015696d7772dd9e1/Events/CombatEventCapture.cs#L81
        this.ActorControlHandler = scanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64");
    }
}