// <copyright file="Server_EffectResultEntry.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Protocol;

using System.Runtime.InteropServices;

// EffectResultN has byte NumEntries at offset 0 and array EffectResultEntry[N] at offset 4
[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal unsafe struct Server_EffectResult
{
    public uint RelatedActionSequence;
    public uint ActorID;
    public uint CurrentHP;
    public uint MaxHP;
    public ushort CurrentMP;
    public byte RelatedTargetIndex;
    public byte ClassJob;
    public byte DamageShield;
    public byte EffectCount;
    public ushort padding3;
    public fixed byte Effects[4 * 4 * 4]; // Server_EffectResultEffectEntry[4]
}