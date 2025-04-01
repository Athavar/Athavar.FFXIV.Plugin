// <copyright file="SpearfishWindow.cs" company="Athavar">
// Copyright (c) Athavar. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
// </copyright>
#pragma warning disable CS1591
namespace Athavar.FFXIV.Plugin.AutoSpear.SeFunctions;

using System.Runtime.InteropServices;
using Athavar.FFXIV.Plugin.AutoSpear.Enum;
using FFXIVClientStructs.FFXIV.Component.GUI;

// Source: https://github.com/Ottermandias/GatherBuddy/blob/main/GatherBuddy/SeFunctions/SpearfishWindow.cs
[StructLayout(LayoutKind.Explicit)]
internal struct SpearfishWindow
{
    [FieldOffset(0)]
    public AtkUnitBase Base;

    [FieldOffset(0x29C)]
    public Info Fish1;

    [FieldOffset(0x2B8)]
    public Info Fish2;

    [FieldOffset(0x2D4)]
    public Info Fish3;

    public unsafe AtkResNode* FishLines => this.Base.UldManager.NodeList[3];

    public unsafe AtkResNode* Fish1Node => this.Base.UldManager.NodeList[15];

    public unsafe AtkResNode* Fish2Node => this.Base.UldManager.NodeList[16];

    public unsafe AtkResNode* Fish3Node => this.Base.UldManager.NodeList[17];

    [StructLayout(LayoutKind.Explicit)]
    public struct Info
    {
        [FieldOffset(0x00)]
        public bool Available;

        [FieldOffset(0x08)]
        public bool InverseDirection;

        [FieldOffset(0x09)]
        public bool GuaranteedLarge;

        [FieldOffset(0x0A)]
        public SpearfishSize Size;

        [FieldOffset(0x0C)]
        public SpearfishSpeed Speed;
    }
}