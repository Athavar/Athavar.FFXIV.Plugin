// <copyright file="Scanner.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.OpcodeWizard.Models;

using Athavar.FFXIV.Plugin.OpcodeWizard.PacketDetection;

internal sealed class Scanner
{
    public Scanner(Opcode opcodeType, string packetName, string tutorial, Func<IpcPacket, string[], Comment, bool> scanDelegate, Comment comment, string[] parameterPrompts, PacketSource packetSource, Opcode? dependentScanner)
    {
        this.OpcodeType = opcodeType;
        this.PacketName = packetName;
        this.Tutorial = tutorial;
        this.ScanDelegate = scanDelegate;
        this.Comment = comment;
        this.ParameterPrompts = parameterPrompts;
        this.PacketSource = packetSource;
        this.DependentScanner = dependentScanner;
    }

    public ushort Opcode { get; set; }

    public Comment Comment { get; set; }

    public bool Running { get; set; }

    public Opcode OpcodeType { get; init; }

    public string PacketName { get; init; }

    public string Tutorial { get; init; }

    public Func<IpcPacket, string[], Comment, bool> ScanDelegate { get; init; }

    public PacketSource PacketSource { get; init; }

    public string[] ParameterPrompts { get; init; }

    public Opcode? DependentScanner { get; init; }
}