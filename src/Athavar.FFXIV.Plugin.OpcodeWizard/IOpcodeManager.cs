// <copyright file="IOpcodeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.OpcodeWizard;

public interface IOpcodeManager
{
    IReadOnlyDictionary<ushort, Opcode> Opcodes { get; }

    ushort GetOpcode(Opcode opcode);

    void AddOrUpdate(Opcode opcode, ushort value);

    void Remove(Opcode opcode);
}