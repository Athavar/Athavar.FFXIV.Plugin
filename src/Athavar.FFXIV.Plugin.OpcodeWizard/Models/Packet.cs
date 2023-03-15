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