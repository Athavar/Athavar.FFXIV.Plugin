namespace Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

using Athavar.FFXIV.Plugin.OpcodeWizard.Models;
using Dalamud.Logging;

internal static class PacketScanner
{
    /// <summary>
    ///     Returns the opcode of the first packet to meet the conditions outlined by del.
    /// </summary>
    public static ushort Scan(Queue<Packet> pq, Scanner scanner, string[] parameters, ref bool skipped, ref bool stopped, ref bool debug, IReadOnlyDictionary<ushort, Opcode> alreadyKnown)
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

            if (debug)
            {
                if (alreadyKnown.TryGetValue(foundPacket.Opcode, out var code))
                {
                    PluginLog.LogInformation($"{scanner.PacketSource} => {foundPacket.Opcode:x4}[{code}] - Length: {foundPacket.Data.Length}");
                }
                else
                {
                    PluginLog.LogInformation($"{scanner.PacketSource} => {foundPacket.Opcode:x4} - Length: {foundPacket.Data.Length} - Data: {BitConverter.ToString(foundPacket.Data)}");
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
        => new(basePacket.Connection,
            basePacket.Epoch,
            basePacket.Data,
            basePacket.Source,
            BitConverter.ToUInt32(basePacket.Data, Offsets.PacketSize),
            BitConverter.ToUInt16(basePacket.Data, Offsets.SegmentType),
            BitConverter.ToUInt16(basePacket.Data, Offsets.IpcType),
            BitConverter.ToUInt32(basePacket.Data, Offsets.SourceActor),
            BitConverter.ToUInt32(basePacket.Data, Offsets.TargetActor)
        );
}