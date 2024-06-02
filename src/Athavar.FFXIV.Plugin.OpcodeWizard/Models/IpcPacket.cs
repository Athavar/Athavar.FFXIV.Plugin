// <copyright file="IpcPacket.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.Models;

using Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

internal sealed class IpcPacket : Packet
{
    public IpcPacket(string connection, long epoch, byte[] data, PacketSource source, uint packetSize, ushort segmentType, ushort opcode, uint sourceActor, uint targetActor)
        : base(connection, epoch, data, source)
    {
        this.PacketSize = packetSize;
        this.SegmentType = segmentType;
        this.Opcode = opcode;
        this.SourceActor = sourceActor;
        this.TargetActor = targetActor;
    }

    public uint PacketSize { get; set; }

    public ushort SegmentType { get; set; }

    public ushort Opcode { get; set; }

    public uint SourceActor { get; set; }

    public uint TargetActor { get; set; }
}