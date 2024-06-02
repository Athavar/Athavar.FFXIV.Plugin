// <copyright file="Packet.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.Models;

using Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

internal class Packet
{
    public Packet(string connection, long epoch, byte[] data, PacketSource source)
    {
        this.Connection = connection;
        this.Epoch = epoch;
        this.Data = data;
        this.Source = source;
    }

    public string Connection { get; init; }

    public long Epoch { get; init; }

    public byte[] Data { get; init; }

    public PacketSource Source { get; init; }
}