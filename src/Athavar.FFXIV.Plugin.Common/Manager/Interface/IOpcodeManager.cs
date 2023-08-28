// <copyright file="IOpcodeManager.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Athavar.FFXIV.Plugin.Common.Manager.Interface;

public interface IOpcodeManager
{
    IReadOnlyDictionary<ushort, Opcode> Opcodes { get; }

    ushort GetOpcode(Opcode opcode);

    void AddOrUpdate(Opcode opcode, ushort value);

    void Remove(Opcode opcode);
}