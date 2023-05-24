// <copyright file="Server_EffectResultBasicEntry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the GPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Protocol;

using System.Runtime.InteropServices;

// EffectResultBasicN has byte NumEntries at offset 0 and array EffectResultBasicEntry[N] at offset 4
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct Server_EffectResultBasicEntry
{
    public uint RelatedActionSequence;
    public uint ActorID;
    public uint CurrentHP;
    public byte RelatedTargetIndex;
    public byte Padding3;
    public ushort Padding4;
}