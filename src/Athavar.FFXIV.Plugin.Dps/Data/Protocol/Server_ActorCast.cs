// <copyright file="Server_ActorCast.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Athavar.FFXIV.Plugin.Dps.Data.Protocol;

using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game;
using Machina.FFXIV.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
internal struct Server_ActorCast
{
    public Server_MessageHeader MessageHeader; // 8 DWORDS
    public ushort SpellId;
    public ActionType ActionType;
    public byte BaseCastTime100ms;
    public uint ActionId; // also action ID; dissector calls it ItemId - matches actionId of ActionEffectHeader - e.g. when using KeyItem, action is generic 'KeyItem 1', Unknown1 is actual item id, probably similar for stuff like mounts etc.
    public float CastTime;
    public uint TargetID;
    public ushort Rotation;
    public byte Interruptible;
    public byte U1;
    public uint U2ObjID;
    public ushort PosX;
    public ushort PosY;
    public ushort PosZ;
    public ushort U3;
}