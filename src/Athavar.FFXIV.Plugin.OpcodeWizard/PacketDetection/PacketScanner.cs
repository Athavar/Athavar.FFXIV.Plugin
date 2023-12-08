// <copyright file="PacketScanner.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

using Athavar.FFXIV.Plugin.Models.Interfaces;
using Athavar.FFXIV.Plugin.OpcodeWizard.Models;

internal static class PacketScanner
{
    /// <summary>
    ///     Returns the opcode of the first packet to meet the conditions outlined by del.
    /// </summary>
    public static ushort Scan(Queue<Packet> pq, Scanner scanner, string[] parameters, ref bool skipped, ref bool stopped, IPluginLogger? logger, IReadOnlyDictionary<ushort, Opcode> alreadyKnown)
    {
        while (!skipped && !stopped)
        {
            if (pq.Count == 0)
            {
                Thread.Sleep(1);
                continue;
            }

            Packet packet;
            lock (pq)
            {
                packet = pq.Dequeue();
            }

            if (packet.Source != scanner.PacketSource)
            {
                continue;
            }

            var foundPacket = ScanGeneric(packet);

            if (logger is not null)
            {
                if (alreadyKnown.TryGetValue(foundPacket.Opcode, out var code))
                {
                    logger.Information($"{scanner.PacketSource} => {foundPacket.Opcode:x4}[{code}] - Length: {foundPacket.Data.Length}");
                }
                else
                {
                    logger.Information($"{scanner.PacketSource} => {foundPacket.Opcode:x4} - Length: {foundPacket.Data.Length} - Data: {BitConverter.ToString(foundPacket.Data)}");
                }
            }

            if (scanner.ScanDelegate(foundPacket, parameters, scanner.Comment))
            {
                return foundPacket.Opcode;
            }
        }

        return 0;
    }

    /// <summary>
    ///     Pull packets from the queue and do basic parsing on them.
    /// </summary>
    private static IpcPacket ScanGeneric(Packet basePacket)
        => new(
            basePacket.Connection,
            basePacket.Epoch,
            basePacket.Data,
            basePacket.Source,
            BitConverter.ToUInt32(basePacket.Data, Offsets.PacketSize),
            BitConverter.ToUInt16(basePacket.Data, Offsets.SegmentType),
            BitConverter.ToUInt16(basePacket.Data, Offsets.IpcType),
            BitConverter.ToUInt32(basePacket.Data, Offsets.SourceActor),
            BitConverter.ToUInt32(basePacket.Data, Offsets.TargetActor));
}