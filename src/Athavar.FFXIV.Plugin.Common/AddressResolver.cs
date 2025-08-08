// <copyright file="AddressResolver.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
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
    ///     Gets or sets the pointer to the method responsible for receiving agent events.
    /// </summary>
    public nint AgentReceiveEvent { get; set; }

    /// <summary>
    ///     Gets or sets the pointer to the method responsible for receiving ActorControl network packages.
    /// </summary>
    public nint ActorControlHandler { get; set; }

    /// <summary>
    ///     Gets or sets the pointer to the method responsible for receiving EffectResult network packages.
    /// </summary>
    public nint EffectResultHandler { get; set; }

    protected override void Setup64Bit(ISigScanner scanner)
    {
        this.CfPopPacketHandler = scanner.ScanText("40 53 57 48 83 EC 78 48 8B D9 48 8D 0D");

        // this.AgentReceiveEvent = scanner.ScanText("48 89 5C 24 ?? 57 48 83 EC 20 48 8B DA 4D 8B D0 32 D2");

        // from: https://github.com/Kouzukii/ffxiv-deathrecap/blob/75826ebe91713e34578ad4d7ed0cc98f5622154b/Events/CombatEventCapture.cs#L26C1-L33C100
        this.ActorControlHandler = scanner.ScanText("E8 ?? ?? ?? ?? 0F B7 0B 83 E9 64");
        this.EffectResultHandler = scanner.ScanText("48 8B C4 44 88 40 18 89 48 08");
    }
}