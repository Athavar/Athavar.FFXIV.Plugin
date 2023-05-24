// <copyright file="Offsets.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

// https://github.com/SapphireServer/Sapphire/blob/master/src/common/Network/CommonNetwork.h
internal static class Offsets
{
    public const int PacketSize = 0x00;
    public const int SourceActor = 0x04;
    public const int TargetActor = 0x08;
    public const int SegmentType = 0x0C;
    public const int IpcType = 0x12;
    public const int ServerId = 0x16;
    public const int Timestamp = 0x18;
    public const int IpcData = 0x20;
}